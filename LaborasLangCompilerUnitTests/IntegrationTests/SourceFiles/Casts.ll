use System.Collections;
use System;
use System.Text;

entry auto main = void()
{
	auto data = parseArray(Console.ReadLine().Split(char[]{' '}));
	auto out = join(data);
	Console.WriteLine(out);
};

auto parseArray = ArrayList(string[] input)
{
	auto ret = ArrayList();
	for(int i = 0; i < input.Length; i++)
	{
		ret.Add(int.Parse(input[i]));
	}
	return ret;
};

auto join = string(ArrayList input)
{
	string delim = "";
	StringBuilder builder = StringBuilder();
	for(auto obj in input)
	{
		int value = (int)obj;
		builder.Append(delim + value);
		delim = " ";
	}
	return builder.ToString();
};