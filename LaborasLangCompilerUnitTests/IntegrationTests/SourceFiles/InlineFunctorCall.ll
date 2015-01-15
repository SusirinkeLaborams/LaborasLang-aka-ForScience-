use System;
entry auto Main = void()
{
	getFoo()();
};

auto foo = void()
{
	Console.Write("It Works!");
};

auto getFoo = void()()
{
	return foo;
};