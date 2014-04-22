
auto Main = int()
{
	string ret = "";
	auto count = 99;
	while(count > 80)
	{
		ret += bottles(count) + " on the wall\n";
		ret += bottles(count) + "\n";
		ret += "take on down, pass it around\n";
		count -= 1;
		ret += bottles(count);
	}
	System.Console.Write(ret);
	System.Console.ReadKey();
	return 0;
};

auto bottles = string(int count)
{
	auto ret = "" + count;
	if(count % 10 == 1)
	{
		ret = " bottle ";
	}
	else
	{
		ret = " bottles ";
	}
	ret += "of beer";
	return ret;
};