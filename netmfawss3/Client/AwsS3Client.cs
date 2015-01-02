using System;
using System.Net;
using System.Text;
using netmfawss3.Account;
using netmfawss3.Aws;
using netmfawss3.Utilities;

namespace netmfawss3.Client
{
    public class AwsS3Client
    {
        #region fields
        private readonly AwsS3Account _account;
        private readonly Encoding _encoding = new UTF8Encoding();
        private readonly string _currentRegion;
        private readonly Regions _region;
        private readonly string _hostUri;
        private string _timestamp;
        private string _dateTimeStamp;
        #endregion

        private string HttpVerb { get; set; }

        public AwsS3Client(AwsS3Account account, Regions region = Regions.UsEast1)
        {
            _account = account;
            _region = region;
            switch (region)
            {
                case Regions.EuCentral1:
                    _currentRegion = "eu-central-1";
                    break;
                case Regions.UsEast1:
                    _currentRegion = "us-east-1";
                    break;
                case Regions.UsWest2:
                    _currentRegion = "us-west-2";
                    break;
                case Regions.UsWest1:
                    _currentRegion = "us-west-1";
                    break;
                case Regions.EuWest1:
                    _currentRegion = "eu-west-1";
                    break;
                case Regions.ApSoutheast1:
                    _currentRegion = "ap-southeast-1";
                    break;
                case Regions.ApSoutheast2:
                    _currentRegion = "ap-southeast-2";
                    break;
                case Regions.ApNortheast1:
                    _currentRegion = "ap-northeast-1";
                    break;
                case Regions.SaEast1:
                    _currentRegion = "sa-east-1";
                    break;

                default:
                    _currentRegion = "us-east-1";
                    break;
            }

            if (_region != Regions.UsEast1)
            {
                _hostUri = "s3-" + _currentRegion + ".amazonaws.com";
            }
            else
            {
                _hostUri = "s3.amazonaws.com";
            }
        }

        public bool PutBucket(string bucketName)
        {
            HttpVerb = "PUT";
            _timestamp = DateTime.UtcNow.ToString("yyyyMMdd") + "T" +
                        DateTime.UtcNow.ToString("HHmmss") + "Z";
            const string signedHeaders = "host;x-amz-content-sha256;x-amz-date";
            byte[] regionXmlByte;
            if (_region != Regions.UsEast1)
            {
                var regionXml = "<CreateBucketConfiguration xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">" +
                            "<LocationConstraint>" + _currentRegion + "</LocationConstraint>" +
                            "</CreateBucketConfiguration>";
                regionXmlByte = _encoding.GetBytes(regionXml);
            }
            else
            {
                regionXmlByte = _encoding.GetBytes("");
            }
            
            var hashed = SHA.computeSHA256(regionXmlByte);
            var hex = ConvertToString(hashed, 0, hashed.Length);
            var hexHash = hex.Replace("-", string.Empty).ToLower();
            var canonicalHeaders = "host:" + bucketName + ".s3.amazonaws.com:443" + "\n" +
                                   "x-amz-content-sha256:" + hexHash + "\n" +
                                   "x-amz-date:" + _timestamp + "\n";
            var canRequest = CreateCanonicalRequest(HttpVerb, "/", canonicalHeaders, signedHeaders, hexHash);
            var authHeader = CreateAuthorizationHeader("us-east-1", signedHeaders, canRequest);

            var request =
                (HttpWebRequest) WebRequest.Create("https://" + bucketName + ".s3.amazonaws.com/");
            request.Method = HttpVerb;
            request.ContentLength = regionXmlByte.Length;
            request.Headers.Add("x-amz-content-sha256", hexHash);
            request.Headers.Add("x-amz-date", _timestamp);
            request.Headers.Add("Authorization", authHeader);
            using (var stream = request.GetRequestStream())
            {
                stream.Write(regionXmlByte, 0, regionXmlByte.Length);
            }
            using (var response = (HttpWebResponse) request.GetResponse())
            {
                return response.StatusCode == HttpStatusCode.OK;
            }
        }

        public bool DeleteBucket(string bucketName)
        {
            HttpVerb = "DELETE";
            _timestamp = DateTime.UtcNow.ToString("yyyyMMdd") + "T" +
                        DateTime.UtcNow.ToString("HHmmss") + "Z";
            const string signedHeaders = "host;x-amz-date";
            var hashed = SHA.computeSHA256(_encoding.GetBytes(""));
            var hex = ConvertToString(hashed, 0, hashed.Length);
            var hexHash = hex.Replace("-", string.Empty).ToLower();
            var canonicalHeaders = "host:" + _hostUri + ":443" + "\n" +
                                   "x-amz-date:" + _timestamp + "\n";
            var canRequest = CreateCanonicalRequest(HttpVerb, "/" + bucketName, canonicalHeaders, signedHeaders, hexHash);
            var authHeader = CreateAuthorizationHeader(_currentRegion, signedHeaders, canRequest);

            var request =
                (HttpWebRequest) WebRequest.Create("https://" + _hostUri + "/" + bucketName);
            request.Method = HttpVerb;
            request.Headers.Add("x-amz-content-sha256", hexHash);
            request.Headers.Add("x-amz-date", _timestamp);
            request.Headers.Add("Authorization", authHeader);
            using (var response = (HttpWebResponse) request.GetResponse())
            {
                return response.StatusCode == HttpStatusCode.NoContent;
            }
        }

        public bool PutObject(string bucketName, string objectName, byte[] objectByte)
        {
            HttpVerb = "PUT";
            _timestamp = DateTime.UtcNow.ToString("yyyyMMdd") + "T" +
                        DateTime.UtcNow.ToString("HHmmss") + "Z";
            var objectUri = bucketName + "/" + objectName;
            const string signedHeaders = "host;x-amz-content-sha256;x-amz-date";
            var hashed = SHA.computeSHA256(objectByte);
            var hex = ConvertToString(hashed, 0, hashed.Length);
            var hexHash = hex.Replace("-", string.Empty).ToLower();
            var canonicalHeaders = "host:" + _hostUri + ":443" + "\n" +
                                   "x-amz-content-sha256:" + hexHash + "\n" +
                                   "x-amz-date:" + _timestamp + "\n";
            var canRequest = CreateCanonicalRequest(HttpVerb, "/" + objectUri, canonicalHeaders, signedHeaders,
                hexHash);
            var authHeader = CreateAuthorizationHeader(_currentRegion, signedHeaders, canRequest);

            var request =
                (HttpWebRequest) WebRequest.Create("https://" + _hostUri + "/" + objectUri);
            request.Method = HttpVerb;
            request.ContentLength = objectByte.Length;
            request.Headers.Add("x-amz-content-sha256", hexHash);
            request.Headers.Add("x-amz-date", _timestamp);
            request.Headers.Add("Authorization", authHeader);
            using (var stream = request.GetRequestStream())
            {
                stream.Write(objectByte, 0, objectByte.Length);
            }
            using (var response = (HttpWebResponse) request.GetResponse())
            {
                return response.StatusCode == HttpStatusCode.OK;
            }
        }

        public bool DeleteObject(string bucketName, string objectName)
        {
            HttpVerb = "DELETE";
            _timestamp = DateTime.UtcNow.ToString("yyyyMMdd") + "T" +
                        DateTime.UtcNow.ToString("HHmmss") + "Z";
            var objectUri = bucketName + "/" + objectName;
            const string signedHeaders = "host;x-amz-date";
            var hashed = SHA.computeSHA256(_encoding.GetBytes(""));
            var hex = ConvertToString(hashed, 0, hashed.Length);
            var hexHash = hex.Replace("-", string.Empty).ToLower();
            var canonicalHeaders = "host:" + _hostUri + ":443" + "\n" +
                                   "x-amz-date:" + _timestamp + "\n";
            var canRequest = CreateCanonicalRequest(HttpVerb, "/" + objectUri, canonicalHeaders, signedHeaders,
                hexHash);
            var authHeader = CreateAuthorizationHeader(_currentRegion, signedHeaders, canRequest);

            var request =
                (HttpWebRequest) WebRequest.Create("https://" + _hostUri + "/" + objectUri);
            request.Method = HttpVerb;
            request.Headers.Add("x-amz-content-sha256", hexHash);
            request.Headers.Add("x-amz-date", _timestamp);
            request.Headers.Add("Authorization", authHeader);
            using (var response = (HttpWebResponse) request.GetResponse())
            {
                return response.StatusCode == HttpStatusCode.NoContent;
            }
        }


        private static string CreateCanonicalRequest(string httpMethod, string canonicalUri, string canonicalHeaders,
            string signedHeaders, string hashedPayload = "\n")
        {
            var canRequest = httpMethod + "\n" +
                             canonicalUri + "\n" +
                             "\n" +
                             canonicalHeaders + "\n" +
                             signedHeaders + "\n" +
                             hashedPayload;

            return canRequest;
        }

        private string CreateStringToSign(string region, string canonicalRequest)
        {
            var hashed = SHA.computeSHA256(_encoding.GetBytes(canonicalRequest));
            var hex = ConvertToString(hashed, 0, hashed.Length);
            var hexHash = hex.Replace("-", string.Empty).ToLower();

            var stringToSign = "AWS4-HMAC-SHA256" + "\n" +
                               _timestamp + "\n" +
                               _dateTimeStamp + "/" + region + "/s3/aws4_request" + "\n" +
                               hexHash;

            return stringToSign;
        }

        private string CreateSignature(string region, string canonicalRequest)
        {
            var dateKey = SHA.computeHMAC_SHA256(_encoding.GetBytes("AWS4" + _account.AwsSecretAccessKey),
                _encoding.GetBytes(_dateTimeStamp));
            var dateRegionKey = SHA.computeHMAC_SHA256((dateKey), _encoding.GetBytes(region));
            var dateRegionServiceKey = SHA.computeHMAC_SHA256(dateRegionKey, _encoding.GetBytes("s3"));
            var signingKey = SHA.computeHMAC_SHA256(dateRegionServiceKey, _encoding.GetBytes("aws4_request"));

            var signedPolicyBytes = SHA.computeHMAC_SHA256(signingKey,
                _encoding.GetBytes(CreateStringToSign(region, canonicalRequest)));

            return ConvertToString(signedPolicyBytes, 0, signedPolicyBytes.Length).Replace("-", string.Empty).ToLower();
        }

        private string CreateAuthorizationHeader(string region, string signedHeaders, string canonicalRequest)
        {
            _dateTimeStamp = DateTime.UtcNow.ToString("yyyyMMdd");

            return "AWS4-HMAC-SHA256 Credential=" + _account.AwsAccessKeyId + "/" + _dateTimeStamp +
                   "/" + region + "/s3/aws4_request" + ",SignedHeaders=" + signedHeaders + ",Signature=" +
                   CreateSignature(region, canonicalRequest);
        }

        private static string ConvertToString(byte[] value, int index, int length)
        {
            var c = new char[length * 3];
            byte b;

            for (int y = 0, x = 0; y < length; ++y, ++x)
            {
                b = (byte)(value[index + y] >> 4);
                c[x] = (char)(b > 9 ? b + 0x37 : b + 0x30);
                b = (byte)(value[index + y] & 0xF);
                c[++x] = (char)(b > 9 ? b + 0x37 : b + 0x30);
                c[++x] = '-';
            }
            return new string(c, 0, c.Length - 1);
        }
    }
}