namespace CloseConnectv1.Utilities
{
    public static class Constants
    {
        public enum SearchUserRelationshipCodes
        {
            Friends = 0,
            SentRequest = 1,
            ReceivedRequest = 2,
            NotFriends = 3
        }

        private static string mEDIASTACK_BASEURL = "http://api.mediastack.com/v1/news";

        public static string MEDIASTACK_BASEURL { get => mEDIASTACK_BASEURL; set => mEDIASTACK_BASEURL = value; }
    }
}
