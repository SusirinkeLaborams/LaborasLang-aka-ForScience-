use System;
entry auto main = int()
{
	auto data = parseArray(Console.ReadLine().Split());
	auto swap = parseSwap(Console.ReadLine().Split());
	swap(data, swap[0], swap[1]);
	Console.WriteLine(String.Join(" ", data));
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
};

auto parseArray = int[](string[] input)
{
	auto ret = int[input.Length];
	uint i = 0;
	while(i < ret.Length)
	{
		ret[i] = int.Parse(input[i]);
	}
	return ret;
};