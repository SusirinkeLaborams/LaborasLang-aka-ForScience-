﻿auto IsEven = bool(int number)
{
    return number % 2 == 0;
};

auto Func = void(int i)
{
	if (i > 10)
	{
		return;
	}

	if (IsEven(i))
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

entry auto Main = int()
{
	Func(0);
	return 0;
};