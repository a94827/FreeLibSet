﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:2.0.50727.8805
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using System.Text;
using System.Net;

#pragma warning disable 1591
// 
// This source code was auto-generated by wsdl, Version=2.0.50727.42.
// 


[System.Web.Services.WebServiceBindingAttribute(Name = "BasicHttpBinding_IDownloadService",
  Namespace = "https://fias.nalog.ru/WebServices/Public/DownloadService.asmx/")]
public partial class IDownloadService : System.Web.Services.Protocols.SoapHttpClientProtocol
{


  public IDownloadService()
  {
    this.Url = "http://fias.nalog.ru/WebServices/Public/DownloadService.asmx";
    this.RequestEncoding = Encoding.UTF8;
    //this.AllowAutoRedirect = true;
    this.EnableDecompression = true;
    //    this.SoapVersion = SoapProtocolVersion.Soap12;
  }


  [System.Web.Services.Protocols.SoapDocumentMethodAttribute("https://fias.nalog.ru/WebServices/Public/DownloadService.asmx/GetAllDownloadFileInfo",
    RequestNamespace = "https://fias.nalog.ru/WebServices/Public/DownloadService.asmx/",
    ResponseNamespace = "https://fias.nalog.ru/WebServices/Public/DownloadService.asmx/",
    Use = System.Web.Services.Description.SoapBindingUse.Literal,
    ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
  [return: System.Xml.Serialization.XmlArrayAttribute(IsNullable = true)]
  public DownloadFileInfo[] GetAllDownloadFileInfo()
  {
    object[] results = this.Invoke("GetAllDownloadFileInfo", new object[0]);
    return ((DownloadFileInfo[])(results[0]));
  }

  [System.Web.Services.Protocols.SoapDocumentMethodAttribute("https://fias.nalog.ru/WebServices/Public/DownloadService.asmx/GetLastDownloadFileInfo",
    RequestNamespace = "https://fias.nalog.ru/WebServices/Public/DownloadService.asmx/",
    ResponseNamespace = "https://fias.nalog.ru/WebServices/Public/DownloadService.asmx/",
    Use = System.Web.Services.Description.SoapBindingUse.Literal,
    ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
  public DownloadFileInfo GetLastDownloadFileInfo()
  {
    object[] results = this.Invoke("GetLastDownloadFileInfo", new object[0]);
    return ((DownloadFileInfo)(results[0]));
  }


  protected override System.Net.WebRequest GetWebRequest(Uri uri)
  {
    System.Net.WebRequest wr = base.GetWebRequest(uri);
    ((HttpWebRequest)wr).KeepAlive = true;
    wr.Headers.Add("SOAPAction", "https://fias.nalog.ru/WebServices/Public/DownloadService.asmx/GetAllDownloadFileInfo");
    //wr.Headers.Add("Host", "fias.nalog.ru");
    ((HttpWebRequest)wr).MaximumResponseHeadersLength = 1000000;
    ((HttpWebRequest)wr).AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
    ((HttpWebRequest)wr).Accept = "text/xml";
    //((HttpWebRequest)wr).AllowAutoRedirect = true;
    return wr;
  }
}

[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "https://fias.nalog.ru/WebServices/Public/DownloadService.asmx/")]
public partial class DownloadFileInfo
{
  private long versionIdField;

  private string textVersionField;

  private string fiasCompleteDbfUrlField;

  private string fiasCompleteXmlUrlField;

  private string fiasDeltaDbfUrlField;

  private string fiasDeltaXmlUrlField;

  private string kladr4ArjUrlField;

  private string kladr47ZUrlField;

  /// <remarks/>
  public long VersionId
  {
    get
    {
      return this.versionIdField;
    }
    set
    {
      this.versionIdField = value;
    }
  }

  /// <remarks/>
  public string TextVersion
  {
    get
    {
      return this.textVersionField;
    }
    set
    {
      this.textVersionField = value;
    }
  }

  /// <remarks/>
  public string FiasCompleteDbfUrl
  {
    get
    {
      return this.fiasCompleteDbfUrlField;
    }
    set
    {
      this.fiasCompleteDbfUrlField = value;
    }
  }

  /// <remarks/>
  public string FiasCompleteXmlUrl
  {
    get
    {
      return this.fiasCompleteXmlUrlField;
    }
    set
    {
      this.fiasCompleteXmlUrlField = value;
    }
  }

  /// <remarks/>
  public string FiasDeltaDbfUrl
  {
    get
    {
      return this.fiasDeltaDbfUrlField;
    }
    set
    {
      this.fiasDeltaDbfUrlField = value;
    }
  }

  /// <remarks/>
  public string FiasDeltaXmlUrl
  {
    get
    {
      return this.fiasDeltaXmlUrlField;
    }
    set
    {
      this.fiasDeltaXmlUrlField = value;
    }
  }

  /// <remarks/>
  public string Kladr4ArjUrl
  {
    get
    {
      return this.kladr4ArjUrlField;
    }
    set
    {
      this.kladr4ArjUrlField = value;
    }
  }

  /// <remarks/>
  public string Kladr47ZUrl
  {
    get
    {
      return this.kladr47ZUrlField;
    }
    set
    {
      this.kladr47ZUrlField = value;
    }
  }
}


