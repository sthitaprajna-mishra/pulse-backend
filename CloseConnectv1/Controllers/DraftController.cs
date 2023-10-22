using Azure;
using CloseConnectv1.Filters;
using CloseConnectv1.Models;
using CloseConnectv1.Models.DTO;
using CloseConnectv1.Repository.IRepository;
using CloseConnectv1.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace CloseConnectv1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(TokenValidationFilter))]
    public class DraftController : ControllerBase
    {
        private readonly IDraftRepository _dbDraft;
        private readonly DTOConversion _conversion;
        private readonly UserManager<ApplicationUser> _userManager;

        public DraftController(IDraftRepository dbDraft, DTOConversion conversion, UserManager<ApplicationUser> userManager)
        {
            _dbDraft = dbDraft;
            _conversion = conversion;
            _userManager = userManager;
        }

        [HttpGet("GetDrafts/{loginId}")]
        public async Task<IActionResult> GetDrafts(string loginId)
        {
            try
            {
                // Check if user has provided email or username for login
                var isValidEmail = StaticHelpers.CheckIfEmail(loginId);

                ApplicationUser? existingUser;

                // Check if the user exists
                if (isValidEmail) existingUser = await _userManager.FindByEmailAsync(loginId);
                else existingUser = await _userManager.FindByNameAsync(loginId);

                if (existingUser is null) return NotFound();

                List<Draft> draftList = await _dbDraft.GetAllAsync(draft => draft.AuthorId == existingUser.Id);

                draftList.ForEach(draft =>
                {
                    draft.CreateDate = draft.CreateDate.ToLocalTime();
                    draft.UpdateDate = draft.UpdateDate.ToLocalTime();
                });

                draftList = draftList.OrderByDescending(draft => draft.UpdateDate).ToList();

                return Ok(draftList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateDraft")]
        public async Task<IActionResult> CreatePost([FromBody] DraftCreateDTO createDTO)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                if (createDTO == null) return BadRequest();

                Draft model = _conversion.ConvertDraftCreateDTOToDraft(createDTO); 

                await _dbDraft.CreateAsync(model);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateDraft/{draftId:int}")]
        public async Task<IActionResult> UpdateDraft(int draftId, [FromBody] DraftUpdateDTO updateDTO)
        {
            try
            {
                if (!ModelState.IsValid || updateDTO is null) return BadRequest();

                Draft existingDraft = await _dbDraft.GetAsync(d => d.DraftId == draftId, false);

                if (existingDraft is null) return NotFound();

                existingDraft.Content = updateDTO.Content;
                existingDraft.UpdateDate = DateTime.UtcNow;
                existingDraft.CharacterCount = updateDTO.CharacterCount;    

                await _dbDraft.UpdateAsync(existingDraft);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteDraft/{draftId:int}")]
        public async Task<IActionResult> DeleteDraft(int draftId)
        {
            try
            {
                if (draftId == 0) return BadRequest();

                Draft draftToBeDeleted = await _dbDraft.GetAsync(draft => draft.DraftId == draftId);

                if (draftToBeDeleted is null) return NotFound();

                await _dbDraft.RemoveAsync(draftToBeDeleted);
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    
    }
}
