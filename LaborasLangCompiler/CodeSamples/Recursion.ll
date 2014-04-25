auto Func = void(int i)
{
	if (i % 2 == 0)
	{
		System.Console.WriteLine("{0} is even", i);
		Func(i + 3);
	}
	else
	{
		System.Console.WriteLine("{0} is odd", i);
		Func(i + 5);
	}
};

auto Main = int()
{
	Func(0);
	return 0;
};