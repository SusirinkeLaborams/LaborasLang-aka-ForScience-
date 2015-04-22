use System;
use System.IO;
use System.Net;

entry auto Program = void()
{
	auto webResponse = WebRequest.Create("http://info.cern.ch/hypertext/WWW/TheProject.html").GetResponse();
	auto reader = StreamReader(webResponse.GetResponseStream());
	auto html = reader.ReadToEnd();

	reader.Close();
	webResponse.Close();
	
	const auto titleOpeningTag = "<title>";
	const auto titleClosingTag = "</title>";
	auto titleStartIndex = html.IndexOf(titleOpeningTag, StringComparison.InvariantCultureIgnoreCase) + titleOpeningTag.Length;
	auto titleEndIndex = html.IndexOf(titleClosingTag, titleStartIndex, StringComparison.InvariantCultureIgnoreCase);
	auto pageTitle = html.Substring(titleStartIndex, titleEndIndex - titleStartIndex);

    System.Console.WriteLine(pageTitle);
};