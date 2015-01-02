namespace netmfawss3.Account
{
    public class AwsS3Account
    {
        public string AwsAccessKeyId { get; private set; }
        public string AwsSecretAccessKey { get; private set; }

        public AwsS3Account(string awsAccessKeyId, string awsSecretAccessKey)
        {
            AwsAccessKeyId = awsAccessKeyId;
            AwsSecretAccessKey = awsSecretAccessKey;
        }
    }
}