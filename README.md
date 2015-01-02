netmfawss3
==========

This is a .NET Micro Framework Library for the Amazon S3 Rest API. You could do the following things with this library:
* Create a Bucket (Container, Folder)
* Delete a Bucket
* Create an Object (File)
* Delete an Object

As there are many SHA256 calculations inside the library because of the hight security of the AWS services you get problems on your device. So please test it extensive on your device.

##### Credits
Great thanks to Elze Kool (kooldevelopment.nl) which has written the SHA Class which I am working with.

#### Requirements
---
1. .NET Micro Framework 4.3
2. It is very important that you have set the correct time on your device because this is critical to get it working.
3. You should also updates the SSL certificates on your device via MFDeploy.
4. Visual Studio
5. An active Amazon AWS S3 Account
  * You need Access Credentials for your client, which you could generate under "Security Credentials". Go to "Access Keys (Access Key ID and Secret Access Key)" and there you are getting your Access Key ID and your Secret Access Key.


### Example Usage
---
Create a new Instance of the AwsS3Account Class with your credentials:
```c#
var account = new AwsS3Account("<Your Access Key ID>", "<Your Secret Access Key>");
```

After that create an instance of the AwsS3Client Class with the account from above and a Region if you do not want to use the Default US-East-1 Region:
```c#
var client = new AwsS3Client(account, Regions.EuWest1);
```

----

###### Create a new bucket
We are now ready to create our new bucket in the region which was defined above. The Method returns a value of True when it successfully created the bucket:
```c#
if (client.PutBucket(bucketName))
{
  Debug.Print("Bucket successfully created");
}
```

###### Create a new object
We are now ready to create/upload a object to the bucket which we created above. The Method returns a value of True when it successfully created/uploaded the object:
```c#
if (client.PutObject(bucketName, objectName, bytes))
{
  Debug.Print("Object successfully uploaded");
}
```

###### Delete an object
Now we will delete the object which we created above. The Method returns a value of True when it successfully deleted the object:
```c#
if (client.DeleteObject(bucketName, objectName))
{
  Debug.Print("Object successfully deleted");
}
```

###### Delete a bucket
After all our tests are finished we will delete the bucket. The Method returns a value of True when it successfully deleted the bucket:
```c#
if (client.DeleteBucket(bucketName))
{
  Debug.Print("Bucket successfully deleted");
}
```
