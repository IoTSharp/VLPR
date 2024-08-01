# IoTSharp.VLPR
这是一个用于.Net 的车牌识别对接框架库， 如果你要在项目里面对接车牌识别， 可以按项目提供的标准提供给厂家， 让厂家按照接口对接， 然后你只需要拷贝对应动态库到程序目录加载即可使用。 在我们内部， 已经有数家厂家进行了对接。



##  使用

`dotnet add package IoTSharp.VLPR  `



## 配置

再appsettings.json中加入下面内容：

```json
 "VLPROptions": {
    "Interval": 300,
    "EasyVLPR": false,
    "VLPRConfigs": [
      {
        "Name": "1",
        "Provider": "libvpr_xlw.so.1.0.0.3",
        "Port": 5001,
        "IPAddress": "10.13.97.91",
        "Password": "admin",
        "UserName": "admin"
      }
    ]
  },
```

## 示例:

```
namespace Console1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
      
            services.AddVPRService();
          
            services.AddHealthChecks()
                    .AddVLPR("VLPR");

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }
    }
}
```



## 接口对接

### 1.1  车牌识别库要求

>		交付内容， 提供的车牌识别库 命名格式为libVpr_<厂家缩写和型号缩写>.so 有完全测试通过的demo 的代码，包括Makefile 文件。   

### 1.2  Linux共享库（SO）接口定义 ##

#### 1.2.1车牌识别接口识别车牌的基本原理    ###   

#### 1.2.2接口函数说明  ###             					                    

 ** VPR_InitEx   ** 

- 	long VPR_InitEx(char* capIpAddress,char* username, char* password ,int uPort)`

> 		函数说明           													 
> 		用于初始化系统中的车牌照识别器。  
> 		参数			 说明      		                                    
> 		uPort         用户自定义端口，当接口库接收到车牌照识别器的识别结果时，向这个端口发送数据包。  
> 		username     用户名   
> 		password      密码   
> 		capIpAddress  车牌识别IP地址   
> 		返回值			说明       												      
> 		<=0       	  错误，0、网络错误 -1， 登录失败、-2、其他错误
> 		>0          句柄

- `BOOL VPR_Quit(long Handle)`

> 		函数说明             													
> 		用于关闭系统中的车牌照识别器。
> 		参数			说明    												      
> 		无
> 		返回值			说明    												      
> 		TRUE    	  关闭牌照识别器成功
> 		FALSE		  关闭牌照识别器失败   



- `int VPR_GetVehicleInfoEx (long Handle, char *pchPlate, unsigned char *iPlateColor,int *piByteBinImagLen,BYTE *pByteBinImage,int *piJpegImageLen,unsigned char* pByteJpegImage, int* laneId, int* index);`   

> 		函数说明：           												
> 		获取车牌号、车牌二值图、车辆JPEG图像。
> 		接口与调用说明: DLL_VPR_SetEventCallBackFunc 


> | 参数             | 说明                                             |
> | ---------------- | ------------------------------------------------ |
> | pchPlate         | 返回牌照号                                       |
> | iPlateColor      | 车牌颜色                                         |
> | piByteBinImagLen | 返回车辆二值图的大小                             |
> | pByteBinImage    | 返回车牌二值图                                   |
> | piJpegImageLen   | 返回车辆JPEG图像的大小                           |
> | pByteJpegImage   | 返回车辆的图片, 为JPEG格式                       |
> | laneId           | 输出参数,车道号,对应VPR_CaptureEx中传入的车道号, |
> | index            | 输出参数,抓拍ID,对应VPR_CaptureEx中传入的抓拍ID  |
> | 返回值           | 说明                                             |
> | ---              | ---                                              |
> | TRUE             | 返回车辆的图片, 为JPEG格式                       |
> | FALSE            | 获取信息失败                                     |

> | iPlateColor 值 | 对应颜色 |
> | -------------- | -------- |
> | 0              | 蓝       |
> | 1              | 黄       |
> | 2              | 黑       |
> | 3              | 白       |
> | 4              | 渐绿     |
> | 5              | 黄绿     |
> | 6              | 蓝白     |
>
> ##### VPR_GetVehicleInfoEx接口必读注意事项:  
>
> 1. 函数返回时，若没有识别结果，pchPlate将返回“无车牌”，iPlateColor 返回0，pchPlate至少申请30字节的空间。
> 2. pByteBinImage  最大为280字节的空间。
> 3. pByteJpegImage 最大为 1024  * 1024 * 2 字节的空间。
> 4. pchPlate   最大长度为 64字节。
> 5. 中文编码采用GB2312编码。


- `int VPR_CaptureEx (long Handle,  int laneId, int index )`  

> 函数说明：当上位机调用该接口时，该接口作为指令触发方式，告知接口需要取一次车牌信息，车牌识别仪收到后进行抓拍。  
> 	参数			说明    												      
> 	Handle  		相机句柄，由VPR_Init 或 VPR_InitEx 接口返回
> 	laneId			车道号，需与相机中配置的相对应
> 	index			抓拍ID，用于区分不同的抓拍结果
> 	返回值			说明      												      
> 	TRUE       	  发送抓拍命令成功
> 	FALSE         发送抓拍命令失败

-`    int VPR_CheckStatus(long Handle, char * chVprDevStatus)`  

> 		```c
> 		函数说明           												
> 		检查牌照识别器状态。  
> 		参数			     说明      												
> 		chVprDevStatus	   牌照识别器状态说明，返回值  最长 512字节，中文编码采用GB2312编码
> 		返回值			     说明      												
> 		TRUE               牌照识别器状态正常  
> 		FALSE              牌照识别器状态不正常  
> 		```

-`    int VPR_SetEventCallBackFunc(long Handle, VPR_EventHandleEx cb)`  

> 		```c
> 		函数说明           												
> 		设置次回调后， 如果收到车牌识别， 则调用 VPR_EventHandleEx  cb      												
> 		typedef void   (*VPR_EventHandleEx)(long Handle,  int laneId, int index);      												
> 		```



