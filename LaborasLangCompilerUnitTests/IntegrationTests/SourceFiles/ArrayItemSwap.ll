use System;
use System.Text;
entry auto main = int()
{
	auto data = parseArray(Console.ReadLine().Split(char[]{' '}));
	auto indices = parseSwap(Console.ReadLine().Split(char[]{' '}));
	swap(data, indices[0], indices[1]);
	string delim = "";
	StringBuilder builder = StringBuilder();
	for(int i = 0; i < data.Length; i++)
	{
		builder.Append(delim + data[i]);
		delim = " ";
	}
	Console.WriteLine(builder.ToString());
	Console.ReadLine();
	return 0;
};

auto swap = void(int[] data, int a, int b)
{
	int tmp = data[a];
	data[a] = data[b];
	data[b] = tmp;
};

auto parseSwap = int[](string[] input)
{
	auto ret = int[2];
	ret[0] = int.Parse(input[0]);
	ret[1] = int.Parse(input[1]);
	return ret;
};

auto parseArray = int[](string[] input)
{
	auto ret = int[input.Length];
	for(int i = 0; i < ret.Length; i++)
	{
		ret[i] = int.Parse(input[i]);
	}
	return ret;
};