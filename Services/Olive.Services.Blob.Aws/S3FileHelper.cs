﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Olive.Entities;

namespace Olive.Services.BlobAws
{
    /// <summary>
    /// A helper class for common S3 actions.
    /// </summary>
    static class S3Proxy
    {
        const string FILE_NOT_FOUND = "NotFound";

        static AmazonS3Client CreateClient() => new AmazonS3Client();

        /// <summary>
        /// Uploads a document to the Amazon S3 Client.
        /// </summary>
        internal static async Task Upload(Blob document)
        {
            using (var client = CreateClient())
            {
                var request = await CreateUploadRequest(document);
                await client.PutObjectAsync(request);
            }
        }

        internal static async Task<bool> FileExists(Blob document)
        {
            try
            {
                using (var client = CreateClient())
                {
                    return await client.GetObjectMetadataAsync(AWSInfo.DocumentsS3BucketName, GetKey(document)) != null;
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.ErrorCode == FILE_NOT_FOUND) return false;

                throw;
            }
        }

        internal static Task<byte[]> Load(Blob document) => Load(GetKey(document));

        internal static async Task<byte[]> Load(string documentKey)
        {
            using (var client = CreateClient())
            {
                var request = CreateGetObjectRequest(documentKey);

                var responseStream = (await client.GetObjectAsync(request)).ResponseStream;

                using (var memoryStream = new MemoryStream())
                {
                    responseStream.CopyTo(memoryStream);

                    return await memoryStream.ReadAllBytesAsync();
                }
            }
        }

        static GetObjectRequest CreateGetObjectRequest(string objectKey)
        {
            return new GetObjectRequest
            {
                BucketName = AWSInfo.DocumentsS3BucketName,
                Key = objectKey
            };
        }

        internal static async Task DeleteOlds(Blob document)
        {
            var oldVersionKeys = await GetOldVersionKeys(document);

            if (oldVersionKeys.Any())
                using (var client = CreateClient())
                {
                    var request = CreateDeleteOldsRequest(oldVersionKeys);
                    await client.DeleteObjectsAsync(request);
                }
        }

        static async Task<IEnumerable<KeyVersion>> GetOldVersionKeys(Blob document)
            => await GetOldKeys(document).Select(s => new KeyVersion { Key = s });

        /// <summary>
        /// Deletes a document with the specified key name on the Amazon S3 server.
        /// </summary>
        internal static async Task Delete(Blob document)
        {
            using (var client = CreateClient())
            {
                var response = await client.DeleteObjectAsync(AWSInfo.DocumentsS3BucketName, GetKey(document));

                Log.Info("S3 Delete Object with key {0} response : httpStatusCode : {1} , responsemetadata : {2}".FormatWith(GetKey(document), response.HttpStatusCode, response.ResponseMetadata.Metadata.Select(i => i.Key + " : " + i.Value).ToString(Environment.NewLine)));
            }
        }

        static string GetKey(Blob document)
        {
            return (document.FolderName + "/" + document.GetFileNameWithoutExtension()).KeepReplacing("//", "/").TrimStart("/");
        }

        static DeleteObjectsRequest CreateDeleteOldsRequest(IEnumerable<KeyVersion> oldKeys)
        {
            return new DeleteObjectsRequest
            {
                BucketName = AWSInfo.DocumentsS3BucketName,
                Objects = oldKeys.ToList()
            };
        }

        static async Task<IEnumerable<string>> GetOldKeys(Blob document)
        {
            var key = GetKey(document);

            using (var client = CreateClient())
            {
                var request = CreateGetObjectsRequest(document);
                return (await client.ListObjectsAsync(request))
                    .S3Objects.Select(s => s.Key);
            }
        }

        static ListObjectsRequest CreateGetObjectsRequest(Blob document)
        {
            var key = GetKey(document);
            var prefix = key.TrimEnd(document.FileExtension);

            return new ListObjectsRequest
            {
                BucketName = AWSInfo.DocumentsS3BucketName,
                Prefix = prefix
            };
        }

        /// <summary>
        /// Creates an Amazon PutObjectRequest for the specified Document and key name.
        /// </summary>
        static async Task<PutObjectRequest> CreateUploadRequest(Blob document)
        {
            return new PutObjectRequest
            {
                BucketName = AWSInfo.DocumentsS3BucketName,
                Key = GetKey(document),
                InputStream = new MemoryStream(await document.GetFileData())
            };
        }
    }
}
