#ifndef LIBVLPR_H
#define LIBVLPR_H



/*
### 1.2.2.1车牌识别接口识别车牌的基本原理
> #### 车牌识别的触发模式：视频触发、线圈触发、指令触发。
> - 视频触发说明：抓拍相机通过视频检测到来车时，完成车牌抓拍，并告知上位机，上位机收到车牌识别仪的告知消息，获取车牌信息。
> - 线圈触发说明：抓拍相机通过线圈检测到来车时，完成车牌抓拍，并告知上位机，上位机收到车牌识别仪的告知消息，获取车牌信息。
> - 指令触发说明(本接口中的VPR_Capture函数)：当上位机调用该函数时，车牌识别仪完成车牌抓拍，并告知上位机，上位机收到车牌识别仪的告知消息，获取车牌信息。

*/

#ifdef __cplusplus
extern "C"
{
#endif

 /*!
 \brief 设置次回调后， 如果收到车牌识别， 则调用 VPR_EventHandle
*/
    typedef void   (*VPR_EventHandle)(long handle, void* userData);

/*!
 \brief

 \fn VPR_InitEx
 \param capIpAddress
 \param username
 \param password
 \param uPort
 \return int
*/
long VPR_InitEx(char* capIpAddress,char* username, char* password ,int uPort);

/*!
 \brief                         用于初始化系统中的车牌照识别器

 \fn VPR_Init
 \param uPort             用户自定义端口，当接口库接收到车牌照识别器的识别结果时，向这个端口发送数据包
 \param nHWYPort      内部接口， 基本不需要
 \param chDevIp         车牌识别IP地址
 \return int
                    TRUE       	  初始化牌照识别器成功;
                    FALSE         初始化牌照识别器失败
*/
long VPR_Init(unsigned int uPort, int nHWYPort, char *chDevIp);


/*!
 \brief                 用于关闭系统中的车牌照识别器

 \fn VPR_Quit
 \return int
                    TRUE    	  关闭牌照识别器成功;
                    FALSE		  关闭牌照识别器失败
*/
int VPR_Quit(long handle);


/*!
 \brief     获取车牌号、车牌二值图、车辆JPEG图像
             函数返回时，若没有识别结果，pchPlate将返回“无车牌”，iPlateColor 返回0，pchPlate至少申请30字节的空间

> | iPlateColor 值  |   对应颜色  |
> | --- | --- |
> |0| 蓝|
> |1| 黄|
> |2| 黑|
> |3| 白|
> |4| 渐绿|
> |5| 黄绿|
> |6| 蓝白|

 \fn VPR_GetVehicleInfo
 \param pchPlate                    返回牌照号, 最大长度为 64字节, 中文编码采用GB2312编码
 \param iPlateColor                 车牌颜色
 \param piByteBinImagLen      返回车辆二值图的大小
 \param pByteBinImage           返回车牌二值图, 最大为280字节的空间
 \param piJpegImageLen         返回车辆JPEG图像的大小
 \param pByteJpegImage         返回车辆的图片, 为JPEG格式 ,最大为 1024  * 1024 * 2 字节的空间
 \return int
 TRUE     | 获取信息成功
 FALSE     | 获取信息失败
*/
int VPR_GetVehicleInfo (long handle, char *pchPlate,
                        unsigned char *iPlateColor,
                        int *piByteBinImagLen,
                        unsigned char *pByteBinImage,
                        int *piJpegImageLen,
                        unsigned char* pByteJpegImage);


/*!
 \brief 当上位机调用该接口时，该接口作为指令触发方式，告知接口需要取一次车牌信息，车牌识别仪收到后进行抓拍

 \fn VPR_Capture
 \return int
 TRUE       	  发送抓拍命令成功
 FALSE         发送抓拍命令失败
*/
int VPR_Capture (long handle);



/*!
 \brief   比较2个车牌二值化图

 \fn VPR_ComparePlateBin
 \param lpBinImageIn        入口车牌二值化图(用户必须申请至少280字节的空间)
 \param lpBinImageOut     出口车牌二值化图(用户必须申请至少280字节的空间)
 \return int
    TRUE          匹配
    FALSE         不匹配
*/
int VPR_ComparePlateBin( unsigned char* lpBinImageIn,unsigned char* lpBinImageOut);


/*!
 \brief     检查牌照识别器状态

 \fn VPR_CheckStatus
 \param chVprDevStatus      牌照识别器状态说明，返回值  最长 512字节
 \return int
    TRUE               牌照识别器状态正常
    FALSE              牌照识别器状态不正常
*/
int VPR_CheckStatus(long handle, char * chVprDevStatus);


/*!
 \brief 设置次回调后， 如果收到车牌识别， 则调用 VPR_EventHandle  cb

 \fn VPR_SetEventCallBackFunc
 \param cb
 \return int
*/
int  VPR_SetEventCallBackFunc(long handle, VPR_EventHandle cb, void* userData);

#ifdef __cplusplus
}
#endif


#endif 
