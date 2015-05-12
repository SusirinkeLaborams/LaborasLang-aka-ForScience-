use System;
use System.Collections;

entry auto Main = void()
{
    for (auto item in System.Collections.ArrayList({1, 2, 3}))
         Console.WriteLine(item);
};