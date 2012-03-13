using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using System.Runtime.Serialization.Json;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;


namespace MezeoFileSupport
{
    public class LoginDetails
    {
        public String szUserName;
        public int nAccountType;

        public double dblStorage_Allocated;
        public double dblStorage_Used;

        public String szS3_Authid;
        public String szs3_Authkey;

        public double dblBandWidth_Allocated;
        public double dblBandWidth_Total;
        public double dblBandWidth_Public;
        public double dblBandwidth_Private;

        public String szContainerContentsUri;
        public String szManagementUri;
        public String szNamespaceUri;
        public String szNQParentUri;

        public LoginDetails()
        {
            szUserName = "";
            nAccountType = -2;

            dblStorage_Allocated = 0.0;
            dblStorage_Used = 0.0;

            szS3_Authid = "";
            szs3_Authkey = "";

            dblBandWidth_Allocated = 0.0;
            dblBandWidth_Total = 0.0;
            dblBandWidth_Public = 0.0;
            dblBandwidth_Private = 0.0;

            szContainerContentsUri = "";
            szManagementUri = "";
            szNamespaceUri = "";
            szNQParentUri = "";
        }
    };

    public class ItemDetails
    {
        public String szItemType;
        public String szContentUrl;
        public String szParentUrl;

        public double dblSizeInBytes;
        public String strName;
        public String strETag;

        public DateTime dtAccessed;
        public DateTime dtCreated;
        public DateTime dtModified;

        public bool bShared;
        public bool bPublic;

        public String szMimeType;

        public int nCurrentPosition;
        public int nTotalItem;

        public ItemDetails()
        {
            szItemType = "";
            szContentUrl = "";
            szParentUrl = "";

            dblSizeInBytes = 0.0;
            strName = "";
            strETag = "";

            bShared = false;
            bPublic = false;

            szMimeType = "";

            nCurrentPosition = -1;
            nTotalItem = -1;
        }
    };

    public class ItemResults
    {
        public String szContentsUrl;
        public String szName;

        public ItemResults()
        {
            szContentsUrl = "";
            szName = "";
        }
    };

    public class NQDetails
    {
        public int nTotalNQ;
        public String StrEventResult;
        public String StrEventTime;
        public String StrObjectID;
        public String StrEventUser;
        public String StrEvent;
        public String StrParentID;
        public String StrObjectType;
        public String StrParentUri;
        public String StrDomainUri;
        public String StrObjectName;
        public String StrMezeoExportedPath;
        public String StrHash;
        public long lSize;

        public NQDetails()
        {
            nTotalNQ = -1;
            StrEventResult = "";
            StrEventTime = "";
            StrObjectID = "";
            StrEventUser = "";
            StrEvent = "";
            StrParentID = "";
            StrObjectType = "";
            StrParentUri = "";
            StrDomainUri = "";
            StrObjectName = "";
            StrMezeoExportedPath = "";
            StrHash = "";
            lSize = 0;
        }
    };

    public class NSResult
    {
        public DateTime dtAccessed;
        public DateTime dtCreated;
        public DateTime dtModified;
        public double dblSizeInBytes;
        public String StrName;
        public String StrVersion;
        public String StrParentUri;
        public String StrContentsUri;
        public String StrType;
        public String StrMimeType;
        public bool bShared;
        public bool bPublic;

        public NSResult()
        {
            dblSizeInBytes = 0;
            StrName = "";
            bShared = false;
            bPublic = false;
            StrVersion = "";
            StrParentUri = ""; 
            StrContentsUri = "";
            StrType = "";
            StrMimeType = "";
        }
    };

    public class MezeoFileCloud
    {
        private String m_strLoginName;
		private String m_strPassword;
        private XmlDocument m_xmlDocument = new XmlDocument();
        private bool m_bStop = false;
        private bool m_bPause = false;
        private String StrAPIKey = "c5f5c39e22b4c743ff7c83470499748c6ac46b249c29e3934f5744166af130c6";

        //create request format for get details
        private void OnGetRequest(ref HttpWebRequest webRequest, String strRequestURL, String strAccept, String strXCloudDepth, String strMethod)
        {
	        webRequest = (HttpWebRequest)WebRequest.Create( strRequestURL );
	        webRequest.Credentials = new NetworkCredential(m_strLoginName, m_strPassword);
	        webRequest.PreAuthenticate = true;
	        webRequest.Method = strMethod;
	        webRequest.KeepAlive = false;
	        webRequest.Headers.Add("X-Client-Specification", "2");
	        if(strXCloudDepth != "")
		        webRequest.Headers.Add("X-Cloud-Depth", strXCloudDepth);
	        webRequest.Accept = strAccept;
            webRequest.Timeout = System.Threading.Timeout.Infinite;
            webRequest.Headers.Add("X-Cloud-Key", StrAPIKey);
        }

        //responce string for request basic
        private String OnGetResponseString(Stream responseStream)
        {
	        StringBuilder responseString = new StringBuilder();
	        byte[] buffer = new byte[4096];
	        int bytes_read = 0;
	        while ((bytes_read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
	        {
		        responseString.Append(Encoding.UTF8.GetString(buffer, 0, bytes_read));
	        }
	        responseStream.Close();
	        return responseString.ToString();
        }

        //notification queue info
        private void OnNotificationQueue(ref HttpWebRequest webRequest, String StrUri, String StrMethod)
        {
            webRequest = (HttpWebRequest)WebRequest.Create(StrUri);
            webRequest.Credentials = new NetworkCredential(m_strLoginName, m_strPassword);
            webRequest.PreAuthenticate = true;
            webRequest.Method = StrMethod;
            webRequest.KeepAlive = false;
            webRequest.Headers.Add("X-CDMI-Specification-Version", "1.0.1");
            webRequest.Accept = "application/cdmi-queue";
            webRequest.ContentType = "application/cdmi-queue";
            webRequest.Timeout = System.Threading.Timeout.Infinite;
            webRequest.Headers.Add("X-Cloud-Key", StrAPIKey);
        }

        private void OnPostAndPutRequest(ref HttpWebRequest webRequest, String strRequestURL, String strSource, String strContentType, String strLoadHeader, String strFinalBoundary, String StrMethod, String strDest)
        {
	        webRequest = (HttpWebRequest)WebRequest.Create( strRequestURL );
	        webRequest.Credentials = new NetworkCredential(m_strLoginName, m_strPassword);
	        webRequest.PreAuthenticate = true;
	        webRequest.Method = StrMethod;
	        webRequest.KeepAlive = false;
	        webRequest.Headers.Add("X-Client-Specification", "2");
            if(strDest != "")
                webRequest.Headers.Add("Content-Location", strDest);
	        webRequest.ContentType = strContentType;
	        //webRequest.Timeout = 86400000;
            webRequest.Timeout = System.Threading.Timeout.Infinite;
            webRequest.Headers.Add("X-Cloud-Key", StrAPIKey);

            Stream writeStream = webRequest.GetRequestStream();
	        if( writeStream != null)
	        {
		        byte[] bytes = Encoding.UTF8.GetBytes(strLoadHeader);
		        writeStream.Write(bytes, 0, bytes.Length);
		        if(strSource != "")
		        {
                    FileStream fileStream = new FileStream(strSource, FileMode.Open, FileAccess.Read);
			        if( fileStream != null)
			        {
				        byte[] buffer = new byte[4096];
				        int bytesRead = 0;

				        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
				        {
					        writeStream.Write(buffer, 0, bytesRead);
				        }
				        fileStream.Close();
			        }
			        bytes = Encoding.UTF8.GetBytes(strFinalBoundary);
			        writeStream.Write(bytes, 0, bytes.Length);
		        }
		        writeStream.Close();
	        }
        }

        //save on the local drive
        private bool OnSaveResponseFile(Stream responseStream, String strSaveInFile, long lFrom)
        {
	        byte[] buffer = new byte[4096];
	        int bytes_read = 0;
	        FileStream fstPersons;

	        if(lFrom > 0)
	        {
		        fstPersons = new FileStream(strSaveInFile, FileMode.Append);
		        fstPersons.Seek(lFrom, SeekOrigin.Begin);
	        }
	        else
		        fstPersons = new FileStream(strSaveInFile, FileMode.Create);
	
	        while ((bytes_read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
	        {
		        fstPersons.Write(buffer, 0, bytes_read);
		        if(m_bStop)
		        {
			        m_bStop = false;
			        return false;
		        }
		        if(m_bPause)
			        return false;
	        }
	        responseStream.Close();
	        fstPersons.Close();

	        return true;
        }

        public void StopSyncProcess()
        {
	        m_bStop = true;
        }

        public void PauseSyncProcess()
        {
	        m_bPause = true;
        }

        public void ResumeSyncProcess()
        {
	        m_bPause = false;
        }

        private String OnGetMimeType(String StrFilePath)
        {
	        String StrMimeType = "";
            FileInfo fileInfo = new FileInfo(StrFilePath);

            RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(fileInfo.Extension.ToLower());
            if (regKey != null)
            {
		        Object ObjContentType = regKey.GetValue("Content Type");
                if (ObjContentType != null)
			        StrMimeType = ObjContentType.ToString();
            }
     
	        return StrMimeType;
        }

        //get exception for item get
        private int OnGetException(WebException webEx)
        {
	        if (webEx.Status == WebExceptionStatus.NameResolutionFailure)
		        return -2;
	        else if(webEx.Status == WebExceptionStatus.ProtocolError)
	        {
		        if(((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.NotFound)
			        return 404;
		        else if(((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.NotAcceptable)
			        return 406;
		        else if(((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.Conflict)
			        return 409;
                else if (((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.NotModified)
                    return 304;
	        }
	        return -1;
        }

        private int OnPostException(WebException webEx)
        {
	        if (webEx.Status == WebExceptionStatus.NameResolutionFailure)
		        return -2;
	        else if(webEx.Status == WebExceptionStatus.ProtocolError)
	        {
		        if(((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.UnsupportedMediaType)
			        return 415;
		        else if(((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.Conflict)
			        return 409;
	        }
	        return -1;
        }

        private int OnDeleteException(WebException webEx)
        {
	        if (webEx.Status == WebExceptionStatus.NameResolutionFailure)
		        return -2;
	        else if(webEx.Status == WebExceptionStatus.ProtocolError)
	        {
		        if(((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.NotFound)
			        return 404;
		        else if(((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.Conflict)
			        return 409;
	        }
	        return -1;
        }

        private int OnPutException(WebException webEx)
        {
	        if (webEx.Status == WebExceptionStatus.NameResolutionFailure)
		        return -2;
	        else if(webEx.Status == WebExceptionStatus.ProtocolError)
	        {
		        if(((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.NotFound)
			        return 404;
		        else if(((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.UnsupportedMediaType)
			        return 415;
		        else if(((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.Conflict)
			        return 409;
	        }
	        return -1;
        }

        private void OnPutRequest(ref HttpWebRequest webRequest, String strRequestURL, String strSource)
        {
	        webRequest = (HttpWebRequest)WebRequest.Create( strRequestURL );
	        webRequest.Credentials = new NetworkCredential(m_strLoginName, m_strPassword);
	        webRequest.PreAuthenticate = true;
	        webRequest.Method = "PUT";
	        webRequest.KeepAlive = false;
	        webRequest.Headers.Add("X-Client-Specification", "2");
            webRequest.Timeout = System.Threading.Timeout.Infinite;
            webRequest.Headers.Add("X-Cloud-Key", StrAPIKey);

            Stream writeStream = webRequest.GetRequestStream();
	        if( writeStream != null)
	        {
                FileStream fileStream = new FileStream(strSource, FileMode.Open, FileAccess.Read);
		        if(fileStream != null)
		        {
			        byte[] buffer = new byte[4096];
			        int bytesRead = 0;

			        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
			        {
				        writeStream.Write(buffer, 0, bytesRead);
			        }
			        fileStream.Close();
		        }
		        writeStream.Close();
	        }
        }

        //Cloud Login
        public LoginDetails Login(String strLoginName, String strPassword, String strUrl, ref int nStatusCode)
        {
            String m_strXmlResource;
            nStatusCode = 0;
	        m_strLoginName = strLoginName;
	        m_strPassword = strPassword;
            LoginDetails pLoginDetails = null;
            
	        try
	        {
		        HttpWebRequest webRequest = null;
                OnGetRequest(ref webRequest, strUrl, "application/vnd.csp.cloud2+xml", "1", "Get");
		        //OnGetRequest(ref webRequest, strUrl, "", "1", "Get");

		        HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

		        nStatusCode = 200;
                m_strXmlResource = OnGetResponseString(response.GetResponseStream());
		        webRequest.Abort();
		        response.Close();

		        if(m_strXmlResource.Substring(0, 6) != "<cloud")
                    return null;

		        m_xmlDocument.LoadXml(m_strXmlResource);

		        XmlNode nodeXml = m_xmlDocument.SelectSingleNode("/cloud/account/account-info/username");
		        pLoginDetails = new LoginDetails();
		        pLoginDetails.szUserName = nodeXml.InnerText;
		
		        nodeXml.RemoveAll();
		        nodeXml = m_xmlDocument.SelectSingleNode("/cloud/account/account-info/account_type");
		        pLoginDetails.nAccountType = Convert.ToInt32(nodeXml.InnerText);

		        nodeXml.RemoveAll();
		        nodeXml = m_xmlDocument.SelectSingleNode("/cloud/account/account-info/storage/allocated");
		        pLoginDetails.dblStorage_Allocated = Convert.ToInt32( nodeXml.InnerText );

		        nodeXml.RemoveAll();
		        nodeXml = m_xmlDocument.SelectSingleNode("/cloud/account/account-info/storage/used");
		        pLoginDetails.dblStorage_Used = Convert.ToInt32(nodeXml.InnerText);

		        nodeXml.RemoveAll();
		        nodeXml = m_xmlDocument.SelectSingleNode("/cloud/account/account-info/s3/authid");
		        pLoginDetails.szS3_Authid = nodeXml.InnerText;

		        nodeXml.RemoveAll();
		        nodeXml = m_xmlDocument.SelectSingleNode("/cloud/account/account-info/s3/authkey");
		        pLoginDetails.szs3_Authkey = nodeXml.InnerText;

		        nodeXml.RemoveAll();
		        nodeXml = m_xmlDocument.SelectSingleNode("/cloud/account/account-info/bandwidth/allocated");
		        pLoginDetails.dblBandWidth_Allocated = Convert.ToInt32(nodeXml.InnerText);

		        nodeXml.RemoveAll();
		        nodeXml = m_xmlDocument.SelectSingleNode("/cloud/account/account-info/bandwidth/total");
		        pLoginDetails.dblBandWidth_Total = Convert.ToDouble( nodeXml.InnerText );

		        nodeXml.RemoveAll();
		        nodeXml = m_xmlDocument.SelectSingleNode("/cloud/account/account-info/bandwidth/public");
		        pLoginDetails.dblBandWidth_Public = Convert.ToDouble(nodeXml.InnerText);

		        nodeXml.RemoveAll();
		        nodeXml = m_xmlDocument.SelectSingleNode("/cloud/account/account-info/bandwidth/private");
		        pLoginDetails.dblBandwidth_Private = Convert.ToDouble(nodeXml.InnerText);

		        nodeXml.RemoveAll();
		        nodeXml = m_xmlDocument.SelectSingleNode("/cloud/rootContainer/container/contents");
		        pLoginDetails.szContainerContentsUri = nodeXml.Attributes["xlink:href"].Value;

		        nodeXml.RemoveAll();
                nodeXml = m_xmlDocument.SelectSingleNode("/cloud/locations/location/management");
		        if(nodeXml != null)
                    pLoginDetails.szManagementUri = nodeXml.Attributes["xlink:href"].Value;

                nodeXml.RemoveAll();
                nodeXml = m_xmlDocument.SelectSingleNode("/cloud/namespaces/namespaces/namespace/container");
                pLoginDetails.szNamespaceUri = nodeXml.Attributes["xlink:href"].Value;
		
		        response.Close();
	        }
	        catch(WebException wEx)
	        {
		        nStatusCode = OnGetException(wEx);
		        return null;
	        }
	        catch
	        {
		        nStatusCode = -3;
		        //delete pLoginDetails;
		        return null;
	        }
	
	        return pLoginDetails;
        }

        //item detials
        public ItemDetails[] DownloadItemDetails(String strContainer, ref int nStatusCode)
        {
            nStatusCode=0;
            String m_strTemp;
	        ItemDetails[] pItemDetails = null;

            DateTime tmStaticSet = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	        //DateTime tmObj;
	        try
	        {
		        HttpWebRequest webRequest = null;
		        OnGetRequest(ref webRequest, strContainer, "", "1", "Get");

		        HttpWebResponse response = (HttpWebResponse)(webRequest.GetResponse());
		        nStatusCode = 200;
		        m_strTemp = OnGetResponseString(response.GetResponseStream());

		        XmlDocument m_xmlFileList = new XmlDocument();
		        m_xmlFileList.LoadXml(m_strTemp);

		        XmlNode nodeXml = m_xmlFileList.SelectSingleNode("/file-list");
		        if(nodeXml.ChildNodes.Count == 0)
			        return null;

		        pItemDetails = new ItemDetails[nodeXml.ChildNodes.Count];
                pItemDetails[0] = new ItemDetails();
                m_strTemp = response.Headers.Get("eTag");

                pItemDetails[0].nTotalItem = -3;

		        webRequest.Abort();
		        response.Close();

		        if(nodeXml.HasChildNodes)
		        {
			        XmlNode nodeChildXml;
			        String strItemType;
			        for(int nNodePos=0; nNodePos<nodeXml.ChildNodes.Count; nNodePos++)
			        {
				        m_xmlFileList.RemoveAll();
				        m_xmlFileList.LoadXml(nodeXml.ChildNodes[nNodePos].InnerXml);
				        strItemType = "";

                        pItemDetails[nNodePos] = new ItemDetails();

                        pItemDetails[0].strETag = m_strTemp;
                        pItemDetails[nNodePos].nTotalItem = nodeXml.ChildNodes.Count;
				        pItemDetails[nNodePos].nCurrentPosition = nNodePos;

                        nodeChildXml = m_xmlFileList.SelectSingleNode("/container");
				        if(nodeChildXml != null)
				        {
					        strItemType = "/container";
					        pItemDetails[nNodePos].szItemType = "DIRECTORY";

					        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/contents");
					        pItemDetails[nNodePos].szContentUrl = nodeChildXml.Attributes["xlink:href"].Value;
					        nodeChildXml.RemoveAll();
				        }
				
                        nodeChildXml = m_xmlFileList.SelectSingleNode("/file");
				        if(nodeChildXml != null)
				        {
					        strItemType = "/file";
					        pItemDetails[nNodePos].szItemType = "FILE";

					        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/mime_type");
					        pItemDetails[nNodePos].szMimeType = nodeChildXml.InnerText;
					        nodeChildXml.RemoveAll();

					        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/public");
                            if(nodeChildXml.InnerText == "True")
					            pItemDetails[nNodePos].bPublic = true;
                            else
                                pItemDetails[nNodePos].bPublic = false;

					        nodeChildXml.RemoveAll();

					        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/content");
					        pItemDetails[nNodePos].szContentUrl = nodeChildXml.Attributes["xlink:href"].Value;
					        nodeChildXml.RemoveAll();
				        }

                        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/accessed");
                        //tmObj = tmStaticSet.AddMilliseconds(Convert.ToInt64(nodeChildXml.InnerText)*1000).ToLocalTime();
                        //pItemDetails[nNodePos].dtAccessed = tmObj.ToString("M/d/yyyy h:mm tt");
                        pItemDetails[nNodePos].dtAccessed = tmStaticSet.AddMilliseconds(Convert.ToInt64(nodeChildXml.InnerText) * 1000).ToLocalTime();
				        nodeChildXml.RemoveAll();

				        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/bytes");
				        pItemDetails[nNodePos].dblSizeInBytes = Convert.ToDouble(nodeChildXml.InnerText);
				        nodeChildXml.RemoveAll();

				        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/created");
                        pItemDetails[nNodePos].dtCreated = tmStaticSet.AddMilliseconds(Convert.ToInt64(nodeChildXml.InnerText) * 1000).ToLocalTime();
				        nodeChildXml.RemoveAll();

				        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/modified");
                        pItemDetails[nNodePos].dtModified = tmStaticSet.AddMilliseconds(Convert.ToInt64(nodeChildXml.InnerText) * 1000).ToLocalTime();
				        nodeChildXml.RemoveAll();

				        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/name");
				        pItemDetails[nNodePos].strName = nodeChildXml.InnerText;
				        nodeChildXml.RemoveAll();

				        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/shared");
                        if (nodeChildXml.InnerText == "True")
                            pItemDetails[nNodePos].bShared = true;
                        else
                            pItemDetails[nNodePos].bShared = false;
				        nodeChildXml.RemoveAll();

				        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/parent");
				        pItemDetails[nNodePos].szParentUrl = nodeChildXml.Attributes["xlink:href"].Value;
				        nodeChildXml.RemoveAll();
			        }
		        }
	        }
	        catch(WebException wEx)
	        {
		        nStatusCode = OnGetException(wEx);
		        return null;
	        }
	        catch
	        {
		        if(pItemDetails != null)
			        pItemDetails[0].nTotalItem -= 1;
		        else
		        {
			        nStatusCode = -3;
			        return null;
		        }
	        }

	        return pItemDetails;
        }

        public bool DownloadFile(String strSource, String strDestination, ref int nStatusCode)
        {
            nStatusCode = 0;
	        try
	        {
		        HttpWebRequest webRequest = null;
		        OnGetRequest(ref webRequest, strSource, "", "1", "Get");

		        HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
		        nStatusCode = 200;
                OnSaveResponseFile(response.GetResponseStream(), strDestination, 0);
			    webRequest.Abort();
			    response.Close();
	        }
	        catch(WebException wEx)
	        {
		        nStatusCode = OnGetException(wEx);
		        return false;
	        }
	        catch
	        {
		        nStatusCode = -3;
		        return false;
	        }

	        return true;
        }

        public String GetETag(String strContainUrl, ref int nStatusCode)
        {
            nStatusCode = 0;
	        String StrRet = "";
	        try
	        {
                int nTypeLen = strContainUrl.LastIndexOf('/') + 1;
                String strBuff = strContainUrl.Substring(nTypeLen, strContainUrl.Length - nTypeLen);
                if (strBuff == "content" || strBuff == "contents")
                    strContainUrl = strContainUrl.Substring(0, nTypeLen-1);

		
		        //Get Parent Url
		        HttpWebRequest webRequest = null;
		        OnGetRequest(ref webRequest, strContainUrl, "", "", "Get");

		        HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
		        nStatusCode = 200;
	
		        XmlDocument m_xmlSingleFile = new XmlDocument();
		        m_xmlSingleFile.LoadXml(OnGetResponseString(response.GetResponseStream()));
		        webRequest.Abort();
		        response.Close();
		
		        XmlNode nodeXml = m_xmlSingleFile.SelectSingleNode("/file");
		        if(nodeXml != null)
		        {
			        nodeXml = m_xmlSingleFile.SelectSingleNode("/file/parent");
			        StrRet = nodeXml.Attributes["xlink:href"].Value;
			        nodeXml.RemoveAll();
		        }

                nodeXml = m_xmlSingleFile.SelectSingleNode("/container");
		        if(nodeXml != null)
		        {
			        nodeXml = m_xmlSingleFile.SelectSingleNode("/container/parent");
			        StrRet = nodeXml.Attributes["xlink:href"].Value;
			        nodeXml.RemoveAll();
		        }
		        m_xmlSingleFile.RemoveAll();

		        //check url infomation
		        OnGetRequest(ref webRequest, StrRet, "", "", "Get");
		        response = (HttpWebResponse)webRequest.GetResponse();		
		        m_xmlSingleFile.LoadXml(OnGetResponseString(response.GetResponseStream()));
		        webRequest.Abort();
		        response.Close();
		        m_xmlSingleFile.RemoveAll();

		        //get eTag
		        strContainUrl += "/" + strBuff;
		        OnGetRequest(ref webRequest, strContainUrl, "", "", "Get");
		        response = (HttpWebResponse)webRequest.GetResponse();
		        StrRet = response.Headers.Get("eTag");
		        webRequest.Abort();
		        response.Close();
	        }
	        catch(WebException wEx)
	        {
		        nStatusCode = OnGetException(wEx);
		        return "";
	        }
	        catch
	        {
		        nStatusCode = -3;
		        return "";
	        }

	        return StrRet;
        }

        public ItemDetails GetContinerResult(String strContainUrl, ref int nStatusCode)
        {
            nStatusCode = 0;
	        ItemDetails pItemDetails = null;
	        DateTime tmStaticSet = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            //DateTime tmObj;
	        try
	        {
		        HttpWebRequest webRequest = null;
                OnGetRequest(ref webRequest, strContainUrl, "", "", "Get");

		        HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
		        nStatusCode = 200;

                //String str1 = OnGetResponseString(response.GetResponseStream());

		        XmlDocument m_xmlFileList = new XmlDocument();
                m_xmlFileList.LoadXml(OnGetResponseString(response.GetResponseStream()));

		        webRequest.Abort();
		        response.Close();

		        pItemDetails = new ItemDetails();
		        String strItemType = "";
		        XmlNode nodeChildXml;
                //pItemDetails.strETag = response.Headers.Get("eTag");

                nodeChildXml = m_xmlFileList.SelectSingleNode("/container");
		        if( nodeChildXml != null )
		        {
			        strItemType = "/container";
			        pItemDetails.szItemType = "DIRECTORY";

			        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/contents");
			        pItemDetails.szContentUrl = nodeChildXml.Attributes["xlink:href"].Value;
			        nodeChildXml.RemoveAll();
		        }
				
                nodeChildXml = m_xmlFileList.SelectSingleNode("/file");
		        if( nodeChildXml != null )
		        {
			        strItemType = "/file";
			        pItemDetails.szItemType = "FILE";

			        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/mime_type");
			        pItemDetails.szMimeType = nodeChildXml.InnerText;
			        nodeChildXml.RemoveAll();

			        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/public");
                    if( nodeChildXml.InnerText == "True")
			            pItemDetails.bPublic = true;
                    else
                        pItemDetails.bPublic = false;
			        nodeChildXml.RemoveAll();

			        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/content");
			        pItemDetails.szContentUrl = nodeChildXml.Attributes["xlink:href"].Value;
			        nodeChildXml.RemoveAll();
		        }

                nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/accessed");
                pItemDetails.dtAccessed = tmStaticSet.AddMilliseconds(Convert.ToInt64(nodeChildXml.InnerText) * 1000).ToLocalTime();
		        nodeChildXml.RemoveAll();

		        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/bytes");
		        pItemDetails.dblSizeInBytes = Convert.ToDouble( nodeChildXml.InnerText );
		        nodeChildXml.RemoveAll();

		        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/created");
                pItemDetails.dtCreated = tmStaticSet.AddMilliseconds(Convert.ToInt64(nodeChildXml.InnerText) * 1000).ToLocalTime();
		        nodeChildXml.RemoveAll();

		        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/modified");
                pItemDetails.dtModified = tmStaticSet.AddMilliseconds(Convert.ToInt64(nodeChildXml.InnerText) * 1000).ToLocalTime();
		        nodeChildXml.RemoveAll();

		        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/name");
		        pItemDetails.strName = nodeChildXml.InnerText;
		        nodeChildXml.RemoveAll();

		        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/shared");
                if (nodeChildXml.InnerText == "True")
                    pItemDetails.bShared = true;
                else
                    pItemDetails.bShared = false;
		        nodeChildXml.RemoveAll();

		        nodeChildXml = m_xmlFileList.SelectSingleNode(strItemType + "/parent");
		        pItemDetails.szParentUrl = nodeChildXml.Attributes["xlink:href"].Value;
		        nodeChildXml.RemoveAll();
	        }
	        catch(WebException wEx)
	        {
		        nStatusCode = OnGetException(wEx);
		        return null;
	        }
	        catch
	        {
		        if(pItemDetails != null)
			        pItemDetails.nTotalItem -= 1;
		        else
		        {
			        nStatusCode = -3;
			        return null;
		        }
	        }

	        return pItemDetails;
        }

        private class OnJSONHelper
        {
            public static T Deserialise<T>(string szJson)
            {
                MemoryStream MemStream = new MemoryStream(Encoding.UTF8.GetBytes(szJson));
                DataContractJsonSerializer JSONSerialiser = new DataContractJsonSerializer(typeof(T));
                return (T)JSONSerialiser.ReadObject(MemStream);
            }
        }

        [DataContract]
        private class OnGetParentUri
        {
            [DataMember]
            public string objectName { get; set; }

            [DataMember]
            public string parentURI { get; set; }
        }

        [DataContract]
        private class OnGetNQValue
        {
            [DataMember(Name = "value")]
            public List<String> value { get; set; }
        }

        [DataContract]
        private class OnGetNQQueueValue
        {
            [DataMember]
            public string queueValues { get; set; }
        }

        [DataContract]
        private class GetValue
        {
            [DataMember]
            public string cdmi_event_result { get; set; }
            [DataMember]
            public string cdmi_event_time { get; set; }
            [DataMember]
            public string objectID { get; set; }
            [DataMember]
            public string cdmi_event_user { get; set; }
            [DataMember]
            public string cdmi_event { get; set; }
            [DataMember]
            public string parentID { get; set; }
            [DataMember]
            public string objectType { get; set; }
            [DataMember]
            public string parentURI { get; set; }
            [DataMember]
            public string domainURI { get; set; }
            [DataMember]
            public string objectName { get; set; }
            [DataMember]
            public OnMetadata metadata { get; set; }
        }

        [DataContract]
        public class OnMetadata
        {
            [DataMember]
            public string mezeo_exported_path { get; set; }
            [DataMember]
            public string cdmi_hash { get; set; }
            [DataMember]
            public string cdmi_size { get; set; }
        }

        public Int32 NQGetLength(String StrUri, String StrQueueName, ref int nStatusCode)
        {
            nStatusCode = 0;
            StrUri += "/" + StrQueueName + "?queueValues";
            
            String StrRev;
            Int32 nRet = 0;
            try
            {
                HttpWebRequest webRequest = null;
                OnNotificationQueue(ref webRequest, StrUri, "GET");

                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                nStatusCode = 200;
                StrRev = OnGetResponseString(response.GetResponseStream());

                webRequest.Abort();
                response.Close();

                OnGetNQQueueValue pGetNQQueueValue = OnJSONHelper.Deserialise<OnGetNQQueueValue>(StrRev);
                nRet = pGetNQQueueValue.queueValues.IndexOf('-') ;
                StrRev = pGetNQQueueValue.queueValues.Substring(0, nRet);
                nRet++;
                nRet = Convert.ToInt32( pGetNQQueueValue.queueValues.Substring(nRet, pGetNQQueueValue.queueValues.Length - nRet) );
                nRet -= Convert.ToInt32(StrRev);
                nRet += 1;
            }
            catch (WebException wEx)
            {
                nStatusCode = OnGetException(wEx);
                return 0;
            }
            catch
            {
                nStatusCode = -3;
                return 0;
            }
            return nRet;
        }

        public NQDetails[] NQGetData(String StrUri, String StrQueueName, Int32 nCountValue, ref int nStatusCode)
        {
            nStatusCode = 0;
            StrUri += "/" + StrQueueName +"?values:" + Convert.ToString(nCountValue);
            //StrUri += "/" + StrQueueName;
            //StrUri += "/" + StrQueueName + "?queueValues";
            //StrUri += "/sync.queue";
            //StrUri += "/sync.queue?values:01";
            //StrUri += "/sync.queue?queueValues&valuerange&mimetype&valuetransferencoding&value:0-1";  //blank
            //StrUri += "/sync.queue?mimetype;valuerange;values:20";

            String StrRev;
            NQDetails[] pNQDetails = null;
            try
            {
                HttpWebRequest webRequest = null;
                OnNotificationQueue(ref webRequest, StrUri, "GET");

                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                nStatusCode = 200;
                StrRev = OnGetResponseString(response.GetResponseStream());

                webRequest.Abort();
                response.Close();

                OnGetNQValue pGetNQValue = OnJSONHelper.Deserialise<OnGetNQValue>(StrRev);
                GetValue pGetValue;
                int nCount = pGetNQValue.value.Count();
                if (nCount > 0)
                {
                    pNQDetails = new NQDetails[nCount];
                    for (int i = 0; i < nCount; i++)
                    {
                        pGetValue = OnJSONHelper.Deserialise<GetValue>(pGetNQValue.value[i].ToString());
                        if (pGetValue != null)
                        {
                            pNQDetails[i] = new NQDetails();
                            pNQDetails[i].StrEventResult = pGetValue.cdmi_event_result;
                            pNQDetails[i].StrEventTime = pGetValue.cdmi_event_time;
                            pNQDetails[i].StrObjectID = pGetValue.objectID;
                            pNQDetails[i].StrEventUser = pGetValue.cdmi_event_user;
                            pNQDetails[i].StrEvent = pGetValue.cdmi_event;
                            pNQDetails[i].StrParentID = pGetValue.parentID;
                            if (pGetValue.objectType == "application/cdmi-container")
                                pNQDetails[i].StrObjectType = "DIRECTORY";
                            else
                                pNQDetails[i].StrObjectType = "FILE";
                            pNQDetails[i].StrParentUri = pGetValue.parentURI;
                            pNQDetails[i].StrDomainUri = pGetValue.domainURI;
                            pNQDetails[i].StrObjectName = pGetValue.objectName;
                            pNQDetails[i].StrMezeoExportedPath = pGetValue.metadata.mezeo_exported_path;
                            if (pGetValue.metadata.cdmi_hash == null)
                                pNQDetails[i].StrHash = "";
                            else
                                pNQDetails[i].StrHash = pGetValue.metadata.cdmi_hash;
                            pNQDetails[i].lSize = Convert.ToInt32(pGetValue.metadata.cdmi_size);
                            pNQDetails[i].nTotalNQ = nCount;
                        }
                        pGetValue = null;
                    }
                }
            }
            catch (WebException wEx)
            {
                nStatusCode = OnGetException(wEx);
                return null;
            }
            catch
            {
                nStatusCode = -3;
                return null;
            }
            return pNQDetails;
        }

        public bool NQCreate(String StrUri, String StrQueueName, String StrStarts, ref int nStatusCode)
        {             
            String StrJSON = "{\"metadata\":{\"cdmi_queue_type\":\"cdmi_notification_queue\",";
            StrJSON += "\"cdmi_notification_events\":[\"cdmi_create_complete\",\"cdmi_modify_complete\",\"cdmi_delete\",";
            StrJSON += "\"cdmi_rename\",\"cdmi_copy\"],\"cdmi_scope_specification\":[{\"parentURI\":\"starts ";
            StrJSON += StrStarts;
            StrJSON += "\"}],\"cdmi_results_specification\":{\"cdmi_event\":\"\",\"cdmi_event_result\":\"\",";
            StrJSON += "\"cdmi_event_time\":\"\",\"cdmi_event_user\":\"\",\"objectName\":\"\",\"objectID\":\"\",\"objectType\":\"\",\"parentURI\":\"\",";
            StrJSON += "\"parentID\":\"\",\"domainURI\":\"\",\"metadata\":{\"mezeo_exported_path\":\"\",\"cdmi_size\":\"\",\"cdmi_hash\":\"\"}}}}";
            

            StrUri += "/" + StrQueueName;

            try
            {
                HttpWebRequest webRequest = null;
                webRequest = (HttpWebRequest)WebRequest.Create(StrUri);
                webRequest.Credentials = new NetworkCredential(m_strLoginName, m_strPassword);
                webRequest.PreAuthenticate = true;
                webRequest.Method = "PUT";
                webRequest.KeepAlive = false;
                webRequest.Headers.Add("X-CDMI-Specification-Version", "1.0.1");
                webRequest.ContentType = "application/cdmi-queue";
                webRequest.Accept = "application/cdmi-queue";
                webRequest.Timeout = System.Threading.Timeout.Infinite;
                webRequest.Headers.Add("X-Cloud-Key", StrAPIKey);

                Stream writeStream = webRequest.GetRequestStream();
                if (writeStream != null)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(StrJSON);
                    writeStream.Write(bytes, 0, bytes.Length);
                    writeStream.Close();
                }
                nStatusCode = 202;
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

                webRequest.Abort();
                response.Close();
            }
            catch (WebException wEx)
            {
                nStatusCode = OnGetException(wEx);
                return false;
            }
            catch
            {
                nStatusCode = -3;
                return false;
            }

            return true;
        }

        public bool NQDeleteValue(String StrUri, String StrQueueName, int nCountValue, ref int nStatusCode)
        {
            StrUri += "/" + StrQueueName + "?values:" + Convert.ToString( nCountValue );

            try
            {
                HttpWebRequest webRequest = null;
                OnNotificationQueue(ref webRequest, StrUri, "DELETE");

                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                nStatusCode = 200;

                webRequest.Abort();
                response.Close();

            }
            catch (WebException wEx)
            {
                nStatusCode = OnGetException(wEx);
                return false;
            }
            catch
            {
                nStatusCode = -3;
                return false;
            }
            return true;
        }

        public String NQParentUri(String StrUri, ref int nStatusCode)
        {
            String StrRev;
            OnGetParentUri pGetParentUri;
            try
            {
                HttpWebRequest webRequest = null;
                OnNotificationQueue(ref webRequest, StrUri, "GET");

                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                nStatusCode = 200;
                StrRev = OnGetResponseString(response.GetResponseStream());

                webRequest.Abort();
                response.Close();

                pGetParentUri = new OnGetParentUri();
                pGetParentUri = OnJSONHelper.Deserialise<OnGetParentUri>(StrRev);
            }
            catch (WebException wEx)
            {
                nStatusCode = OnGetException(wEx);
                return "";
            }
            catch
            {
                nStatusCode = -3;
                return "";
            }
            return pGetParentUri.parentURI + pGetParentUri.objectName;
        }

        public String NewContainer(String strNewContainer, String strContentsResource, ref int nStatusCode)
        {
            String StrRef = "";
            nStatusCode = 0;
	        String strXML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
	        strXML += "<container xmlns:xlink=\"http://www.w3.org/1999/xlink\">";
	        strXML += "<modified>0</modified>";
	        strXML += "<name>";
	        strXML += strNewContainer;
	        strXML += "</name>";
	        strXML += "<created>0</created>";
	        strXML += "</container>";

	        try
	        {
		        HttpWebRequest webRequest = null;
		        OnPostAndPutRequest(ref webRequest, strContentsResource, "", "application/vnd.csp.container-info+xml", strXML, "", "POST", "");

		        HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
		        nStatusCode = 201;
		        StrRef = OnGetResponseString(response.GetResponseStream());
                StrRef = StrRef.Substring(0, (StrRef.Length - Environment.NewLine.Length));
		        webRequest.Abort();
		        response.Close();
	        }
	        catch(WebException wEx)
	        {
		        nStatusCode = OnPostException(wEx);
		        return "";
	        }
	        catch
	        {
		        nStatusCode = -3;
		        return "";
	        }

	        return StrRef;
        }

        public String UploadingFile(String strSource, String strDestination, ref int nStatusCode)
        {
            nStatusCode = 0;
            String StrRet = "";
	        String strFileName = strSource;

            int nNameLoc = strSource.LastIndexOf('\\');

            if (nNameLoc > 0)
		        strFileName = strSource.Substring(nNameLoc);

	        // Create a new random form boundary using the System time
	        String strBoundary = DateTime.Now.Ticks.ToString("x") + DateTime.Now.Ticks.ToString("x");

	        // File payload section
            String strLoadHeader = "--" + strBoundary + "\r\n";
            strLoadHeader += "Content-Disposition: form-data; name=\"Filedata\"; filename=\"" + strFileName + "\"\r\n";
            strLoadHeader += "Content-Type: " + OnGetMimeType(strSource) + "\r\n\r\n";

	        try
	        {
		        HttpWebRequest webRequest = null;
                OnPostAndPutRequest(ref webRequest, strDestination, strSource, "multipart/form-data; boundary=" + strBoundary, strLoadHeader, "\r\n--" + strBoundary + "--\r\n", "POST", "");

		        HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
		        nStatusCode = 201;
		        StrRet = OnGetResponseString(response.GetResponseStream());
                StrRet = StrRet.Substring(0, (StrRet.Length - Environment.NewLine.Length));
		        webRequest.Abort();
		        response.Close();
	        }
	        catch(WebException wEx)
	        {
		        nStatusCode = OnPostException(wEx);
		        return "";
	        }
	        catch
	        {
		        nStatusCode = -3;
		        return "";
	        }

            return StrRet;
        }

        public bool OverWriteFile(String strSource, String strDestination, ref int nStatusCode)
        {
            nStatusCode = 0;
	        try
	        {
		        HttpWebRequest webRequest = null;
		        OnPutRequest(ref webRequest, strDestination, strSource);

		        HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
		        nStatusCode = 204;
		        webRequest.Abort();
		        response.Close();
	        }
	        catch(WebException wEx)
	        {
		        nStatusCode = OnPutException(wEx);
		        return false;
	        }
	        catch
	        {
		        nStatusCode = -3;
		        return false;
	        }

	        return true;
        }

        public ItemResults GetParentName(String strContents, ref int nStatusCode)
        {
            nStatusCode = 0;
	       
	        ItemResults pItemResults = new ItemResults();
	        try
	        {
		        HttpWebRequest webRequest = null;
		        OnGetRequest(ref webRequest, strContents, "", "1", "Get");

		        HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

		        nStatusCode = 200;
		        strContents = OnGetResponseString(response.GetResponseStream());
		        webRequest.Abort();
		        response.Close();

		        XmlDocument m_xmlSingleFile = new XmlDocument();
		        m_xmlSingleFile.LoadXml(strContents);

		        XmlNode nodeXml = m_xmlSingleFile.SelectSingleNode("/container/parent");
		        pItemResults.szContentsUrl = nodeXml.Attributes["xlink:href"].Value;
		        nodeXml.RemoveAll();

		        nodeXml = m_xmlSingleFile.SelectSingleNode("/container/name");
		        pItemResults.szName = nodeXml.InnerText;
		        nodeXml.RemoveAll();
	        }
	        catch(WebException wEx)
	        {
		        nStatusCode = OnGetException(wEx);
		        return null;
	        }
	        catch
	        {
		        nStatusCode = -3;
		        return null;
	        }

	        return pItemResults;
        }

        public String GetDeletedInfo(String strContainUrl, ref int nStatusCode)
        {
            nStatusCode = 0;
	        String StrRet = "";
	        try
	        {
		        HttpWebRequest webRequest = null;
		        OnGetRequest(ref webRequest, strContainUrl, "", "", "Get");

		        HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
		        nStatusCode = 200;

		        XmlDocument m_xmlSingleFile = new XmlDocument();
		        m_xmlSingleFile.LoadXml(OnGetResponseString(response.GetResponseStream()));
		        webRequest.Abort();
		        response.Close();
		
		        XmlNode nodeXml = m_xmlSingleFile.SelectSingleNode("/file");
		        if(nodeXml != null)
		        {
			        nodeXml = m_xmlSingleFile.SelectSingleNode("/file/parent");
			        StrRet = nodeXml.Attributes["xlink:href"].Value;
			        nodeXml.RemoveAll();
		        }

                nodeXml = m_xmlSingleFile.SelectSingleNode("/container");
		        if(nodeXml != null)
		        {
			        nodeXml = m_xmlSingleFile.SelectSingleNode("/container/parent");
			        StrRet = nodeXml.Attributes["xlink:href"].Value;
			        nodeXml.RemoveAll();
		        }

	        }
	        catch(WebException wEx)
	        {
		        nStatusCode = OnGetException(wEx);
		        return "";
	        }
	        catch
	        {
		        nStatusCode = -3;
		        return "";
	        }

	        return StrRet;
        }

        public bool FileRename(String strPath, String strNewName, String strMineType, bool bPublic, ref int nStatusCode)
        {
            nStatusCode = 0;
	        String strXML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
	        strXML += "<file xmlns:xlink=\"http://www.w3.org/1999/xlink\">";
	        strXML += "<modified>0</modified>";
	        strXML += "<name>";
	        strXML += strNewName;
	        strXML += "</name>";
	        strXML += "<mime_type>";
	        strXML += strMineType;
	        strXML += "</mime_type>";
	        strXML += "<public>";
	        strXML += Convert.ToString(bPublic);
	        strXML += "</public>";
	        strXML += "<created>0</created>";
	        strXML += "</file>";

	        try
	        {
		        HttpWebRequest webRequest = null;
		        OnPostAndPutRequest(ref webRequest, strPath, "", "application/vnd.csp.file-info+xml", strXML, "", "PUT", "");

		        HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
		        nStatusCode = 204;
		        webRequest.Abort();
		        response.Close();
	        }
	        catch(WebException wEx)
	        {
		        nStatusCode = OnPutException(wEx);
		        return false;
	        }
	        catch
	        {
		        nStatusCode = -3;
		        return false;
	        }
	        return true;
        }

        public bool ContainerRename(String strPath, String strNewName, ref int nStatusCode)
        {
            nStatusCode = 0;
	        String strXML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
	        strXML += "<container xmlns:xlink=\"http://www.w3.org/1999/xlink\">";
	        strXML += "<modified>0</modified>";
	        strXML += "<name>";
	        strXML += strNewName;
	        strXML += "</name>";
	        strXML += "<created>0</created>";
	        strXML += "</container>";

	        try
	        {
		        HttpWebRequest webRequest = null;
		        OnPostAndPutRequest(ref webRequest, strPath, "", "application/vnd.csp.container-info+xml", strXML, "", "PUT", "");

		        HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
		        nStatusCode = 204;
		        webRequest.Abort();
		        response.Close();
	        }
	        catch(WebException wEx)
	        {
		        nStatusCode = OnPutException(wEx);
		        return false;
	        }
	        catch
	        {
		        nStatusCode = -3;
		        return false;
	        }
	
	        return true;
        }

        public bool Delete(String strPath, ref int nStatusCode)
        {
            nStatusCode = 0;
	        try
	        {
		        HttpWebRequest webRequest = null;
		        OnGetRequest(ref webRequest, strPath, "", "", "DELETE");

		        HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
		        nStatusCode = 204;
		        webRequest.Abort();
		        response.Close();
	        }
	        catch(WebException wEx)
	        {
		        nStatusCode = OnDeleteException(wEx);
		        return false;
	        }
	        catch
	        {
		        nStatusCode = -3;
		        return false;
	        }
	
	        return true;
        }

        public bool FileMove(String strPath, String strNewName, String strMineType, bool bPublic, String StrParent, ref int nStatusCode)
        {
            nStatusCode = 0;
            String strXML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            strXML += "<file xmlns:xlink=\"http://www.w3.org/1999/xlink\">";
            strXML += "<name>";
            strXML += strNewName;
            strXML += "</name>";
            strXML += "<mime_type>";
            strXML += strMineType;
            strXML += "</mime_type>";
            strXML += "<public>";
            strXML += Convert.ToString(bPublic);
            strXML += "</public>";
            strXML += "<parent xlink:href=\"";
            strXML += StrParent;
            strXML += "\" xlink:type=\"simple\"> </parent>";
            strXML += "</file>";

            try
            {
                HttpWebRequest webRequest = null;
                OnPostAndPutRequest(ref webRequest, strPath, "", "application/vnd.csp.file-info+xml", strXML, "", "PUT", "");

                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                nStatusCode = 204;
                webRequest.Abort();
                response.Close();
            }
            catch (WebException wEx)
            {
                nStatusCode = OnPutException(wEx);
                return false;
            }
            catch
            {
                nStatusCode = -3;
                return false;
            }
            return true;
        }

        public bool ContainerMove(String strPath, String strNewName, String strMineType, bool bPublic, String StrParent, ref int nStatusCode)
        {
            nStatusCode = 0;
            String strXML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            strXML += "<container xmlns:xlink=\"http://www.w3.org/1999/xlink\">";
            strXML += "<name>";
            strXML += strNewName;
            strXML += "</name>";
            strXML += "<mime_type>";
            strXML += strMineType;
            strXML += "</mime_type>";
            strXML += "<public>";
            strXML += Convert.ToString(bPublic);
            strXML += "</public>";
            strXML += "<parent xlink:href=\"";
            strXML += StrParent;
            strXML += "\" xlink:type=\"simple\"> </parent>";
            strXML += "</container>";

            try
            {
                HttpWebRequest webRequest = null;
                OnPostAndPutRequest(ref webRequest, strPath, "", "application/vnd.csp.container-info+xml", strXML, "", "PUT", "");

                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                nStatusCode = 204;
                webRequest.Abort();
                response.Close();
            }
            catch (WebException wEx)
            {
                nStatusCode = OnPutException(wEx);
                return false;
            }
            catch
            {
                nStatusCode = -3;
                return false;
            }
            return true;
        }

        public String Copy(String strSource, String StrDestination, String StrType, ref int nStatusCode)
        {
            nStatusCode = 0;
            String strXML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            if (StrType == "FILE")
            {
                strXML += "<file xmlns:xlink=\"http://www.w3.org/1999/xlink\">";
                strXML += "</file>";
            }
            else if (StrType == "DIRECTORY")
            {
                strXML += "<container xmlns:xlink=\"http://www.w3.org/1999/xlink\">";
                strXML += "</container>";
            }
            else
                return "";

            String StrRet = "";

            try
            {
                HttpWebRequest webRequest = null;
                OnPostAndPutRequest(ref webRequest, strSource, "", "application/vnd.csp.file-info+xml", strXML, "", "POST", StrDestination);

                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

                StrRet = OnGetResponseString(response.GetResponseStream());
                StrRet = StrRet.Substring(0, (StrRet.Length - Environment.NewLine.Length));
                nStatusCode = 201;
                webRequest.Abort();
                response.Close();
            }
            catch (WebException wEx)
            {
                nStatusCode = OnPutException(wEx);
                return "";
            }
            catch
            {
                nStatusCode = -3;
                return "";
            }
            return StrRet;
        }

        public NSResult GetNamespaceResult(String StrUri, String StrObjectType, ref int nStatusCode)
        {
            NSResult pNSResult;
            String strItemType = "";
            DateTime tmStaticSet = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            try
            {
                HttpWebRequest webRequest = null;
                if (StrObjectType == "FILE")
                {
                    OnGetRequest(ref webRequest, StrUri, "application/vnd.csp.file-info+xml", "", "Get");
                    strItemType = "/file";
                }
                else if (StrObjectType == "DIRECTORY")
                {
                    OnGetRequest(ref webRequest, StrUri, "application/vnd.csp.container-info+xml", "", "Get");
                    strItemType = "/container";
                }

                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                nStatusCode = 200;

                XmlDocument m_xmlList = new XmlDocument();
                m_xmlList.LoadXml( OnGetResponseString(response.GetResponseStream()) );

                webRequest.Abort();
                response.Close();

                pNSResult = new NSResult();
                XmlNode nodeChildXml;
                pNSResult.StrType = StrObjectType;

                nodeChildXml = m_xmlList.SelectSingleNode(strItemType + "/contents");
                if (nodeChildXml != null)
                {
                    pNSResult.StrContentsUri = nodeChildXml.Attributes["xlink:href"].Value;
                    nodeChildXml.RemoveAll();
                }

                nodeChildXml = m_xmlList.SelectSingleNode(strItemType + "/mime_type");
                if (nodeChildXml != null)
                {
                    pNSResult.StrMimeType = nodeChildXml.InnerText;
                    nodeChildXml.RemoveAll();
                }

                nodeChildXml = m_xmlList.SelectSingleNode(strItemType + "/public");
                if (nodeChildXml != null)
                { 
                    if (nodeChildXml.InnerText == "True")
                        pNSResult.bPublic = true;
                    else
                        pNSResult.bPublic = false;
                    nodeChildXml.RemoveAll();
                 }

                nodeChildXml = m_xmlList.SelectSingleNode(strItemType + "/content");
                if (nodeChildXml != null)
                {
                    pNSResult.StrContentsUri = nodeChildXml.Attributes["xlink:href"].Value;
                    nodeChildXml.RemoveAll();
                }

                nodeChildXml = m_xmlList.SelectSingleNode(strItemType + "/accessed");
                pNSResult.dtAccessed = tmStaticSet.AddMilliseconds(Convert.ToInt64(nodeChildXml.InnerText) * 1000).ToLocalTime();
                nodeChildXml.RemoveAll();

                nodeChildXml = m_xmlList.SelectSingleNode(strItemType + "/bytes");
                pNSResult.dblSizeInBytes = Convert.ToDouble(nodeChildXml.InnerText);
                nodeChildXml.RemoveAll();

                nodeChildXml = m_xmlList.SelectSingleNode(strItemType + "/created");
                pNSResult.dtCreated = tmStaticSet.AddMilliseconds(Convert.ToInt64(nodeChildXml.InnerText) * 1000).ToLocalTime();
                nodeChildXml.RemoveAll();

                nodeChildXml = m_xmlList.SelectSingleNode(strItemType + "/modified");
                pNSResult.dtModified = tmStaticSet.AddMilliseconds(Convert.ToInt64(nodeChildXml.InnerText) * 1000).ToLocalTime();
                nodeChildXml.RemoveAll();

                nodeChildXml = m_xmlList.SelectSingleNode(strItemType + "/name");
                pNSResult.StrName = nodeChildXml.InnerText;
                nodeChildXml.RemoveAll();

                nodeChildXml = m_xmlList.SelectSingleNode(strItemType + "/version");
                pNSResult.StrVersion = nodeChildXml.InnerText;
                nodeChildXml.RemoveAll();

                nodeChildXml = m_xmlList.SelectSingleNode(strItemType + "/shared");
                if (nodeChildXml.InnerText == "True")
                    pNSResult.bShared = true;
                else
                    pNSResult.bShared = false;
                nodeChildXml.RemoveAll();

                nodeChildXml = m_xmlList.SelectSingleNode(strItemType + "/parent");
                pNSResult.StrParentUri = nodeChildXml.Attributes["xlink:href"].Value;
                nodeChildXml.RemoveAll();
            }
            catch (WebException wEx)
            {
                nStatusCode = OnGetException(wEx);
                return null;
            }
            catch
            {
                nStatusCode = -3;
                return null;
            }
            return pNSResult;
        }

        public bool GetOverlayRegisteration()
        {
            try
            {
                Process.Start("taskkill.exe", "/f /im explorer.exe");
                Thread.Sleep(1000);
                Process.Start(Environment.SystemDirectory + "\\..\\explorer.exe");      
            }
            catch
            {
                return false;
            }
            return true;
        }

        public String GetETagMatching(String StrUri, String StrETag, ref int nStatusCode)
        {
            String StrRev;
            try
            {
                HttpWebRequest webRequest = null;
                OnGetRequest(ref webRequest, StrUri, "", "", "GET");
                webRequest.Headers.Add("If-None-Match", StrETag);

                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

                StrRev = response.Headers.Get("eTag");

                nStatusCode = 200;
                webRequest.Abort();
                response.Close();
            }
            catch (WebException wEx)
            {
                nStatusCode = OnGetException(wEx);
                return "";
            }
            catch
            {
                nStatusCode = -3;
                return "";
            }

            return StrRev;
        }

        public double GetStorageUsed(String strUrl, ref int nStatusCode)
        {
            nStatusCode = 0;
	        double dblStorageSize = 0;
	        try
	        {
		        HttpWebRequest webRequest = null;
		        OnGetRequest(ref webRequest, strUrl, "", "1", "Get");

		        HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
		        nStatusCode = 200;
		        String StrUsedStorage = OnGetResponseString(response.GetResponseStream());
		        webRequest.Abort();
		        response.Close();

		        if(StrUsedStorage.Substring(0, 6) != "<cloud")
			        return 0;

                XmlDocument m_xmlDocument = new XmlDocument();
		        m_xmlDocument.LoadXml(StrUsedStorage);

		        XmlNode nodeXml = m_xmlDocument.SelectSingleNode("/cloud/account/account-info/storage/used");
		        dblStorageSize = Convert.ToDouble( nodeXml.InnerText );
		        nodeXml.RemoveAll();

		        response.Close();
	        }
	        catch(WebException wEx)
	        {
		        nStatusCode = OnGetException(wEx);
		        return 0;
	        }
	        catch
	        {
		        nStatusCode = -3;
		        return 0;
	        }

	        return dblStorageSize;
        }

        public bool StatusConnection(String strLoginName, String strPassword, String strUrl, ref int nStatusCode)
        {	
            nStatusCode = 0;
	        try
	        {
		        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create( strUrl );
		        webRequest.Credentials = new NetworkCredential(strLoginName, strPassword);
		        HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
		        nStatusCode = 200;
                webRequest.Abort();
		        response.Close();
	        }
	        catch(WebException wEx)
	        {
		        nStatusCode = OnGetException(wEx);
		        return false;
	        }
	        catch
	        {
		        nStatusCode = -3;
		        return false;
	        }

	        return true;
        }

        public bool Logout()
        {
	        try
	        {
		        m_strLoginName = "";
		        m_strPassword = "";
                m_xmlDocument.RemoveAll();
                m_bStop = false;
                m_bPause = false;
	        }
	        catch
	        {
		        return false;
	        }

	        return true;
        }

        public bool DownloadResumeFile(String strSource, String strDestination, long lFrom, long lTo, ref int nStatusCode)
        {
            nStatusCode = 0;
	        try
	        {
		        HttpWebRequest webRequest = null;
		        //OnGetResumeRequest(ref webRequest, strSource, lFrom, lTo);
                OnGetRequest(ref webRequest, strSource, "", "", "GET");
                webRequest.AddRange(lFrom, lTo);

		        HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
		        nStatusCode = 200;
		        if(!OnSaveResponseFile(response.GetResponseStream(), strDestination, lFrom))
		        {
			        webRequest.Abort();
			        response.Close();
		        }
	        }
	        catch(WebException wEx)
	        {
		        nStatusCode = OnGetException(wEx);
		        return false;
	        }
	        catch
	        {
		        nStatusCode = -3;
		        return false;
	        }

	        return true;
        }

        public bool ExceuteEventViewer(String strName)
        {
	        try
	        {
                if (File.Exists(Environment.SystemDirectory + "\\winevt\\Logs\\" + strName + ".evtx"))
                    Process.Start(Environment.SystemDirectory + "\\winevt\\Logs\\" + strName + ".evtx");
                else
                    Process.Start(Environment.SystemDirectory + "\\eventvwr.msc");
	        }   
	        catch
	        {
		        return false;
	        }

	        return true;
        }
        
        public bool AppEventViewer(String StrLogName, String StrLogMsg, int nLevel)
        {
            EventLog eventLog = new EventLog();
	        bool bResult = true;
	        try
	        {
		        if(!EventLog.SourceExists(StrLogName))
		        {
			        EventLog.CreateEventSource(StrLogName, StrLogName);
			        eventLog.Source = StrLogName;
			        eventLog.Log = StrLogName;

			        eventLog.Source = StrLogName;
			        eventLog.WriteEntry("The " + StrLogName + " was successfully initialize component.", EventLogEntryType.Information);
		        }

		        eventLog.Source = StrLogName;
		        switch(nLevel)
		        {
			        case 1:
				        eventLog.WriteEntry(StrLogMsg, EventLogEntryType.Error);
				        break;
			        case 2:
				        eventLog.WriteEntry(StrLogMsg, EventLogEntryType.FailureAudit);
				        break;
			        case 3:
				        eventLog.WriteEntry(StrLogMsg, EventLogEntryType.Information);
				        break;
			        case 4:
				        eventLog.WriteEntry(StrLogMsg, EventLogEntryType.SuccessAudit);
				        break;
			        case 5:
				        eventLog.WriteEntry(StrLogMsg, EventLogEntryType.Warning);
				        break;
			        default:
				        bResult = false;
				        break;
		        }
	        }
	        catch
	        {
		        return false;
	        }
            eventLog.Dispose();
	        eventLog.Close();
	
	        return bResult;	
        }

        public bool CleanEventViewer(String StrLogName)
        {
	        try
	        {
		        EventLog eventLog = new EventLog();
		        eventLog.Log = StrLogName;
		        eventLog.Clear();
		        eventLog.Close();
	        }
	        catch
	        {
		        return false;
	        }
	        return true;
        }

        public byte[] Encrypt(string StrPadded)
        {
            byte[] bStr;
            try
            {
                string szBase = Convert.ToBase64String(ASCIIEncoding.Unicode.GetBytes(StrPadded));
                char[] cBuffer = szBase.ToCharArray();
                Int32 nConvt = 0;
                bStr = new byte[szBase.Length];
                for (int i = 0; i < szBase.Length; i++)
                {
                    nConvt = Convert.ToInt32(cBuffer[i]);
                    nConvt += i + 22;
                    bStr[i] = Convert.ToByte(nConvt);
                }
            }
            catch
            {
                return null;
            }

            return bStr;
        }

        public String Decrypt(byte[] bPadded)
        {
            char szBuffer;
            string szBase = "";
            Int32 nConvt = 0;
            for (int i = 0; i < bPadded.Length; i++)
            {
                nConvt = Convert.ToInt32(bPadded[i]);
                nConvt -= i;
                nConvt -= 22;
                szBuffer = Convert.ToChar(nConvt);
                szBase += Convert.ToString(szBuffer);
            }

            byte[] encodedDataAsBytes = Convert.FromBase64String(szBase);
            string returnValue = ASCIIEncoding.Unicode.GetString(encodedDataAsBytes);
            return returnValue;	        
        }



    }
}
