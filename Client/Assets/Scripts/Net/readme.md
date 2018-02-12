* 类名与文件名相同，命名空间为类名将第一个字母替换为小写
* packet.cs主要定义了包相关的内容
* MLClientInetAddress.cs主要与使用的socket地址格式相关，有部分内容需要处理
* MLClientScoket.cs中与使用的socket有关，有些内容需要处理，其中涉及到跨平台的条件编译内容我全部去除了
* bytestreamtoobject.cs中定义的内容是字节流向TCP_Head对象转换的函数，和TCP_Head对象转字节流的函数
* void* 全部替换成了byte[]
* unsigned char 全部替换成了 Byte
* 自定义类型的指针没有找到好的解决办法，全部用自定义类型对象来代替了
* CSosketEngine.cs中有大量指针与字节流的操作，简单的用数组操作代替了，不知道是否正确