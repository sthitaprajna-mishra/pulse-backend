namespace CloseConnectv1.Utilities
{
    public class PostPopularityCalculator
    {
        private const int MaxLikesWeight = 5;
        private const int MaxCommentsWeight = 3;
        private const int MaxRecencyWeight = 2;

        public int CalculatePopularity(int likes, int comments, int maxLikes, int maxComments, DateTime createdAt)
        {
            // Normalize the values to be between 0 and 1
            double normalizedLikes = Normalize(likes, maxLikes);
            double normalizedComments = Normalize(comments, maxComments);
            double normalizedRecency = Normalize(GetRecency(createdAt), GetMaxRecency());

            // Apply weights to each factor
            double weightedLikes = normalizedLikes * MaxLikesWeight;
            double weightedComments = normalizedComments * MaxCommentsWeight;
            double weightedRecency = normalizedRecency * MaxRecencyWeight;

            // Calculate the overall popularity score
            double popularityScore = weightedLikes + weightedComments + weightedRecency;

            // Round the popularity score to the nearest integer
            int popularity = (int)Math.Round(popularityScore, 0);

            // Ensure the popularity is within the scale of 10
            return Math.Min(popularity, 10);
        }

        private static double Normalize(double value, double maxValue)
        {
            return value / maxValue;
        }

        private static double GetRecency(DateTime createdAt)
        {
            // Calculate the time difference between the post's creation time and the current time
            TimeSpan recency = DateTime.UtcNow - createdAt;
            return recency.TotalSeconds;
        }

        private static double GetMaxRecency()
        {
            // Define the maximum recency duration (e.g., 7 days)
            TimeSpan maxRecency = TimeSpan.FromDays(7);
            return maxRecency.TotalSeconds;
        }
    }

}
