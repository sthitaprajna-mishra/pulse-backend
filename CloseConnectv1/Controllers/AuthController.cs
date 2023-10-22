using CloseConnectv1.Data;
using CloseConnectv1.Models;
using CloseConnectv1.Models.DTO;
using CloseConnectv1.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CloseConnectv1.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly TokenValidator _tokenValidator;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ApplicationDbContext context,
            TokenValidationParameters tokenValidationParameters,
            TokenValidator tokenValidator)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
            _tokenValidationParameters = tokenValidationParameters;
            _tokenValidator = tokenValidator;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<AuthResult> Register([FromBody] UserRegistrationRequestDTO requestDto)
        {
            // Validate the incoming request
            if (ModelState.IsValid)
            {
                // We need to check if email already exists
                var emailExists = await _userManager.FindByEmailAsync(requestDto.Email);

                if (emailExists is not null)
                {
                    return new AuthResult
                    {
                        Result = false,
                        Errors = new List<string>
                        {
                            "EmailAlreadyExists"
                        },
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }

                // We need to check if username already exists
                var userExists = await _userManager.FindByNameAsync(requestDto.UserName);

                if (userExists is not null)
                {
                    return new AuthResult
                    {
                        Result = false,
                        Errors = new List<string>
                        {
                            "UserAlreadyExists"
                        },
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }

                // create new user
                var newUser = new ApplicationUser()
                {
                    Name = requestDto.Name,
                    UserName = requestDto.UserName,
                    DOB = requestDto.DOB,
                    CreateDate = DateTime.UtcNow,
                    Email = requestDto.Email,
                    EmailConfirmed = false,
                    DisplayPictureURL = requestDto.DisplayPictureURL,
                };

                var isCreated = await _userManager.CreateAsync(newUser, requestDto.Password);

                var roleAdded = await _userManager.AddToRoleAsync(newUser, "User");

                if (isCreated.Succeeded && roleAdded.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

                    var emailBody = $"Please confirm your email address <a href=\"#URL#\">Click here</a>";

                    // https://localhost:8080/auth/verifyemail/userid=sdas&code=dasdasd
                    var callbackURL = Request.Scheme + "://" + Request.Host + Url.Action("ConfirmEmail", "Auth", new { userId = newUser.Id, code });

                    var body = emailBody.Replace("#URL#", System.Text.Encodings.Web.HtmlEncoder.Default.Encode(callbackURL));

                    // Send email
                    var result = await StaticHelpers.SendEmail(body, newUser.Email);

                    if (result.Sent)
                    {
                        return new AuthResult
                        {
                            Result = true,
                            StatusCode = StatusCodes.Status200OK
                        };
                    }

                    return new AuthResult
                    {
                        Errors = new List<string>
                        {
                            "Email not sent"
                        },
                        Result = false,
                        StatusCode = StatusCodes.Status500InternalServerError
                    };

                    // Generate the token
                    //var tokenString = await GenerateJwtToken(newUser);

                    //return Ok(tokenString);
                }

                return new AuthResult
                {
                    Errors = new List<string>
                    {
                        "Server Error"
                    },
                    Result = false,
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            return new AuthResult
            {
                Errors = new List<string>
                    {
                        "Bad Request"
                    },
                Result = false,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        [HttpGet]
        [Route("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId is null || code is null)
            {
                return BadRequest(new AuthResult
                {
                    Result = false,
                    Errors = new List<string>
                    {
                        "Invalid email confirmation URL"
                    }
                });
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return BadRequest(new AuthResult
                {
                    Result = false,
                    Errors = new List<string>
                    {
                        "Invalid email params"
                    }
                });
            }

            //code = Encoding.UTF8.GetString(Convert.FromBase64String(code));

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if(result.Succeeded)
            {
                string targetURL = "http://localhost:3000/";
                return Redirect(targetURL);
            }

            var status = "Your email is not confirmed, please try again later.";

            return Ok(status);
        }

        [HttpPost]
        [Route("Login")]
        public async Task<AuthResult> Login([FromBody] UserLoginRequestDTO loginRequest)
        {
            if (ModelState.IsValid)
            {
                // Check if user has provided email or username for login
                var isValidEmail = StaticHelpers.CheckIfEmail(loginRequest.LoginId);

                ApplicationUser? existingUser;

                // Check if the user exists
                if (isValidEmail)
                {
                    existingUser = await _userManager.FindByEmailAsync(loginRequest.LoginId);
                }
                else
                {
                    existingUser = await _userManager.FindByNameAsync(loginRequest.LoginId);
                }

                if (existingUser is null)
                {
                    return new AuthResult
                    {
                        Result = false,
                        Errors = new List<string>
                        {
                            "Invalid payload"
                        },
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }

                var emailConfirmedStatus = existingUser.EmailConfirmed;

                if(!emailConfirmedStatus)
                {
                    return new AuthResult
                    {
                        Result = false,
                        Errors = new List<string>
                        {
                        "Email needs to be confirmed"
                        },
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }

                var isCorrect = await _userManager.CheckPasswordAsync(existingUser, loginRequest.Password);

                if (!isCorrect)
                {
                    return new AuthResult
                    {
                        Result = false,
                        Errors = new List<string>
                        {
                            "Invalid credentials"
                        },
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }

                var jwtToken = await GenerateJwtToken(existingUser);

                return jwtToken;
            }

            return new AuthResult
            {
                Errors = new List<string>
                {
                    "Invalid payload"
                },
                Result = false,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        [HttpPost]
        [Route("RefreshToken")]
        public async Task<AuthResult> RefreshToken([FromBody] TokenRequestDTO tokenRequest)
        {
            if (ModelState.IsValid)
            {
                var result = await VerifyAndGenerateToken(tokenRequest);

                if (result is null)
                {
                    return new AuthResult
                    {
                        Errors = new List<string>
                        {
                            "Invalid tokens"
                        },
                        Result = false,
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }

                return result;
            }

            return new AuthResult
            {
                Errors = new List<string>
                {
                    "Invalid parameters"
                },
                Result = false,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        [HttpGet]
        [Route("CalculateSignature")]
        public GenericResult CalculateSignature()
        {
            var token = Guid.NewGuid().ToString();
            var expire = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();

            using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_configuration.GetSection("ImageKitConfig:IMAGEKITIO_PRIVATE_KEY").Value));
            var inputBytes = Encoding.UTF8.GetBytes(token + expire);
            var signatureBytes = hmac.ComputeHash(inputBytes);
            var signature = BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();

            var response = new
            {
                signature,
                token,
                expire
            };

            return new GenericResult
            {
                Result = response
            };
        }

        [HttpGet]
        [Route("Users")]
        public async Task<IActionResult> FetchUsers()
        {
            try
            {
                string token = HttpContext.Request.Headers["Authorization"].ToString();

                if(token.StartsWith("Bearer "))
                {
                    token = token[7..];
                    if(_tokenValidator.IsTokenExpired(token))
                    {
                        return Forbid();
                    } else
                    {
                        List<ApplicationUser> users = await _userManager.Users.ToListAsync();
                        return Ok(users);
                    }
                } else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<AuthResult> VerifyAndGenerateToken(TokenRequestDTO tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                _tokenValidationParameters.ValidateLifetime = false;

                var tokenInVerification =
                    jwtTokenHandler.ValidateToken(
                        tokenRequest.Token, _tokenValidationParameters, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase);

                    if (result is false)
                    {
                        return null;
                    }
                }

                var utcExpiryDate = long.Parse(tokenInVerification.Claims.
                    FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiryDate = StaticHelpers.UnixTimeStampToDateTime(utcExpiryDate);

                if (expiryDate > DateTime.Now)
                {
                    return new AuthResult
                    {
                        Result = false,
                        Errors = new List<string>
                        {
                            "Expired token"
                        },
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }

                var storedToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);

                if (storedToken is null || storedToken.IsUsed || storedToken.IsRevoked)
                {
                    return new AuthResult
                    {
                        Result = false,
                        Errors = new List<string>
                        {
                            "Invalid tokens"
                        },
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }

                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                if (storedToken.JwtId != jti)
                {
                    return new AuthResult
                    {
                        Result = false,
                        Errors = new List<string>
                        {
                            "Invalid tokens"
                        },
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }

                if (storedToken.ExpiryDate < DateTime.UtcNow)
                {
                    return new AuthResult
                    {
                        Result = false,
                        Errors = new List<string>
                        {
                            "Expired token"
                        },
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }

                storedToken.IsUsed = true;
                _context.RefreshTokens.Update(storedToken);
                await _context.SaveChangesAsync();

                var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);

                return await GenerateJwtToken(dbUser);
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Result = false,
                    Errors = new List<string>
                    {
                        "Server error"
                    },
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        private async Task<AuthResult> GenerateJwtToken(ApplicationUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwtConfig:Secret").Value);

            // Token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())

                }),
                Expires = DateTime.UtcNow.Add(TimeSpan.Parse(_configuration.GetSection("JwtConfig:ExpiryTimeFrame").Value)),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken
            {
                JwtId = token.Id,
                Token = StaticHelpers.RandomStringGeneration(23), //Generate a refresh token
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                IsRevoked = false,
                IsUsed = false,
                UserId = user.Id
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            
            var roleIds = _context.UserRoles.Where(c => c.UserId == user.Id).Select(c => c.RoleId).ToList();

            return new AuthResult
            {
                UserId = user.Id,
                Token = jwtToken,
                RefreshToken = refreshToken.Token,
                Result = true,
                StatusCode = StatusCodes.Status200OK,
                RoleIds = roleIds
            };
        }
    }
}
