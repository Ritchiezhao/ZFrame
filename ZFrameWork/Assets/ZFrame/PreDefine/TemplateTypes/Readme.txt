
// TID 内置，可以做Limit

// 根据包含Enums或者Fields确定是Enum还是Class，Enum不支持继承

// 注释可有可无

// Fields中单个字段的增删改、以及字段顺序的变化生成的运行时Binary都是不兼容的，即需要同时Generate Code和Generate Binary才能正常运行

// 只有继承自TBase的类型对象是可以独立存在的，非继承TBase的类型只能作为字段类型使用

// Default为true的对象是该类型的默认值对象，用于定义各个字段默认值，一个类型只能有一个Default对象（仅用于编辑器）

// 不支持直接定义二维数组，可以用类似Array<Item>，Item = Array<SubItem>的形式定义

// Class和Struct的区别：
// Class的对象都有ID，可以独立存在，不能作为字段类型使用
// Struct用于定义内部结构，其对象没有ID，不能独立存在，只能作为字段类型使用
// Class和Struct内部可以包含Struct的字段形成‘组成关系’，可以包含指向Class对象的TID字段形成‘聚合关系’