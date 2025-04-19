using System;
using System.Collections.Generic;

[Serializable]
public class Serialization<T>
{
    public List<T> target;
    public Serialization(List<T> t) { target = t; }
    public List<T> ToList() => target;
}
