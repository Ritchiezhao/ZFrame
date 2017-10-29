using System;

[AttributeUsage(AttributeTargets.Struct|AttributeTargets.Class)]
public class ExpandSerializeAttribute : System.Attribute 
{
    public string subClassName;
    public ExpandSerializeAttribute(string subClassName) {
        this.subClassName = subClassName;
    }
}


[AttributeUsage(AttributeTargets.Field)]
public class GenFieldAttribute : System.Attribute 
{
}



[AttributeUsage(AttributeTargets.Field|AttributeTargets.Property)]
public class GenUidAttribute : System.Attribute 
{
}

