using System.Runtime.InteropServices;
using System.Threading;

namespace GameNet
{
    struct HeaderStruct
    {
        public const bool SOCKET_CHECK = true;
        /** 数据包最大大小 **/
        public const int SIZE_TCP_BUFFER = 102400;   //mChen edit 20480, fix size bug
        /** 头包大小 **/
        public const int SIZE_PACK_HEAD = 8;
        /** 包头信息大小 **/
        public const int SIZE_PACK_INFO = 4;
        /** 命令信息大小 **/
        public const int SIZE_PACK_COMMAND = 4;
        /** 附加数据最大大小 **/
        public const int SIZE_PACK_DATA = SIZE_TCP_BUFFER - SIZE_PACK_INFO - SIZE_PACK_COMMAND;
    }

    public interface ISocketEngine
    {
        /** 设置Socket接收器 */
        void setSocketEngineSink(ISocketEngineSink pISocketEngineSink);//修改了参数类型
	    /** 链接网络 **/
	    bool connect(string url, int port);
        /** 关闭网络 **/
        bool disconnect();
	    /** 发送数据 **/
	    bool send(int main, int sub, byte[] data, int size);
	    /** 状态判断 **/
	    bool isAlive();
	    /** 发送校验 **/
	    void setTCPValidate(bool send);

        /** 创建 */
        //ISocketEngine create();//修改了返回类型的类型
       
        /** 销毁 */
        void destory(ISocketEngine pISocketEngine);//修改了参数类型去掉了指针
    }
}