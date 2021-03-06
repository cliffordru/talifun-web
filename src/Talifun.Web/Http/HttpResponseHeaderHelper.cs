﻿using System;
using System.Web;

namespace Talifun.Web
{
    public class HttpResponseHeaderHelper : IHttpResponseHeaderHelper
    {
        public const string HttpHeaderAcceptRangesBytes = "bytes";
        protected WebServerType WebServerType { get; private set; }

        public HttpResponseHeaderHelper(WebServerType webServerType)
        {
            WebServerType = webServerType;
        }

        /// <summary>
        /// Set the Status Code and Status Description of the http response.
        /// </summary>
        /// <param name="response">The Http Response.</param>
        /// <param name="httpStatusCode">The status to set.</param>
        public void SendHttpStatusHeaders(HttpResponseBase response, HttpStatusCode httpStatusCode)
        {
            response.StatusCode = (int)httpStatusCode;
            response.StatusDescription = (string)httpStatusCode;
        }

        /// <summary>
        /// Append header to response
        /// </summary>
        /// <remarks>
        /// Seems like appendheader only works with IIS 7
        /// </remarks>
        /// <param name="response"></param>
        /// <param name="httpResponseHeader"></param>
        /// <param name="headerValue"></param>
        public void AppendHeader(HttpResponseBase response, HttpResponseHeader httpResponseHeader, string headerValue)
        {
            var httpResponseHeaderString = (string)httpResponseHeader;
            AppendHeader(response, httpResponseHeaderString, headerValue);
        }

        /// <summary>
        /// Append header to response
        /// </summary>
        /// <remarks>
        /// Seems like appendheader only works with IIS 7
        /// </remarks>
        /// <param name="response"></param>
        /// <param name="httpResponseHeader"></param>
        /// <param name="headerValue"></param>
        public void AppendHeader(HttpResponseBase response, string httpResponseHeader, string headerValue)
        {
            switch (WebServerType)
            {
                case WebServerType.VisualStudio2012:
                case WebServerType.VisualStudio2010:
                case WebServerType.VisualStudio2008:
                case WebServerType.IIS7:
                    response.AppendHeader(httpResponseHeader, headerValue);
                    break;
                default:
                    response.AddHeader(httpResponseHeader, headerValue);
                    break;
            }
        }

        /// <summary>
        /// Set the compression type used in the response.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        /// <param name="responseCompressionType">The compression type to use in the response.</param>
        public void SetContentEncoding(HttpResponseBase response, ResponseCompressionType responseCompressionType)
        {
            if (responseCompressionType != ResponseCompressionType.None)
            {
                AppendHeader(response, HttpResponseHeader.ContentEncoding, responseCompressionType.ToString().ToLower());
            }
        }

        /// <summary>
        /// Tell the browser that it supports resumable requests.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        public void SetResponseResumable(HttpResponseBase response)
        {
            // Tell the client software that we accept Range request
            AppendHeader(response, HttpResponseHeader.AcceptRanges, HttpHeaderAcceptRangesBytes);
        }

        /// <summary>
        /// Make the response cachable by browser and any proxies in between.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        /// <param name="now">The current date time of the system.</param>
        /// <param name="lastModified">The last modified date of the entity.</param>
        /// <param name="etag">The etag of the entity.</param>
        /// <param name="maxAge">The time the entity should live before browser will recheck the freshness of the entity.</param>
        public void SetResponseCachable(HttpResponseBase response, DateTime now, DateTime lastModified, string etag, TimeSpan maxAge)
        {
            //Set the expires header for HTTP 1.0 cliets
            response.Cache.SetExpires(now.Add(maxAge));

            //Proxy and browser can cache response
            response.Cache.SetCacheability(HttpCacheability.Public);

            //Proxy cache should check with orginal server once cache has expired
            response.Cache.AppendCacheExtension("must-revalidate, proxy-revalidate");

            //The date the entity was last modified
            response.Cache.SetLastModified(lastModified);

            //The unique identifier for the entity
            response.Cache.SetETag("\"" + etag + "\"");

            //How often the browser should check that it has the latest version
            response.Cache.SetMaxAge(maxAge);
        }

        /// <summary>
        /// Set the response content type.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        /// <param name="contentType">The content type of the entity.</param>
        public void SetContentType(HttpResponseBase response, string contentType)
        {
            response.ContentType = contentType;
        }

        /// <summary>
        /// Set the headers required to temporary redirect this request to the new specified location.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        /// <param name="location">The location to redirect the current request to.</param>
        /// <remarks>
        /// Further requests should be made to the current request url.
        /// </remarks>
        public void SetTemporaryRedirect(HttpResponseBase response, Uri location)
        {
            SendHttpStatusHeaders(response, HttpStatusCode.TemporaryRedirect);
            AppendHeader(response, HttpResponseHeader.Location, location.ToString());
        }

        /// <summary>
        /// Set the headers required to permanently redirect this request to the new specified location.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        /// <param name="location">The location to redirect the current request to.</param>
        /// <remarks>
        /// Further request should be made to the new specified location.
        /// </remarks>
        public void SetMovedPermanently(HttpResponseBase response, Uri location)
        {
            SendHttpStatusHeaders(response, HttpStatusCode.MovedPermanently);
            AppendHeader(response, HttpResponseHeader.Location, location.ToString());
        }

        /// <summary>
        /// Set the headers required to permanently redirect this request to the new specified location.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        /// <param name="location">The location to set the conical content location to.</param>
        /// <remarks>
        /// Most browsers will ignore this header.
        /// </remarks>
        public void SetContentLocation(HttpResponseBase response, Uri location)
        {
            AppendHeader(response, HttpResponseHeader.ContentLocation, location.ToString());
        }
    }
}
