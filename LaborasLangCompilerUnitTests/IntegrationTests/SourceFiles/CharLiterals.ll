use System;

entry auto Main = int()
{
    auto str = "some, words, separated, by, commas";
    auto snippets = str.Split(char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

    auto i = 0;
    while (i < snippets.Length)
    {
	    Console.WriteLine(snippets[i]);
        i++;
    }

	return 0;
};