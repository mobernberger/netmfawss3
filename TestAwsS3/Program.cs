using System.IO;
using Microsoft.SPOT;
using netmfawss3.Account;
using netmfawss3.Aws;
using netmfawss3.Client;

namespace TestAwsS3
{
    public class Program
    {
        public static void Main()
        {
            Debug.Print("Program started");

            //Set Up the account and the client itself including the region, where you want to work.
            var account = new AwsS3Account("<Your Access Key ID>", "<Your Secret Access Key>");
            var client = new AwsS3Client(account, Regions.EuWest1);

            //Define a name for the Bucket (folder, container) and the object (file) itself
            const string bucketName = "gadgeteer-netmf-test1234";
            const string objectName = "testfile1.txt";

            if (client.PutBucket(bucketName))
            {
                Debug.Print("Bucket successfully created");
            }

            using (var ms = new MemoryStream())
            using (TextWriter tw = new StreamWriter(ms))
            {
                tw.WriteLine("Line 1");
                tw.WriteLine("Line 2");
                tw.WriteLine("Line 3");
                tw.WriteLine("Line 4");
                tw.Flush();
                var bytes = ms.ToArray();

                if (client.PutObject(bucketName, objectName, bytes))
                {
                    Debug.Print("Object successfully uploaded");
                }
            }
            if (client.DeleteObject(bucketName, objectName))
            {
                Debug.Print("Object successfully deleted");
            }

            if (client.DeleteBucket(bucketName))
            {
                Debug.Print("Bucket successfully deleted");
            }
        }
    }
}
