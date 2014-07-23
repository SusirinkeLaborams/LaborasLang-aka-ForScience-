auto Main = int()
{
	auto count = 99;
	System.Console.WriteLine("{0} on the wall, {0}.", bottles(count));

	while ((count -= 1) >= 0)
	{
		System.Console.WriteLine("Take one down and pass it around, {0} on the wall.", bottles(count));
		System.Console.WriteLine();
		System.Console.WriteLine("{0} on the wall, {0}.", bottles(count));
	}
	
	System.Console.WriteLine("Go to the store and buy some more, {0} on the wall.", bottles(99));
	return 0;
};

auto bottles = string(int count)
{
	string ret;

	if (count > 0)
	{
		ret += count;

		if (count > 1)
		{
			ret += " bottles ";
		}
		else
		{
			ret += " bottle ";
		}
	}
	else
	{
		ret += "no more bottles ";
	}

	ret += "of beer";
	return ret;
};