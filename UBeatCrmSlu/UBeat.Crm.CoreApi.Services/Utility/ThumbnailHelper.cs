using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using UBeat.Crm.CoreApi.DomainModel.FileService;

namespace UBeat.Crm.CoreApi.Services.Utility
{
    /// <summary>
    /// 缩略图帮助类
    /// </summary>
    public class ThumbnailHelper
    {
        #region CreateThumbnail
        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="sourcedata">原图数据</param>
        /// <param name="t_width">缩略图宽</param>
        /// <param name="t_height">缩略图高</param>
        /// <param name="filename">文件夹，不含路径和后缀</param>
        /// <param name="tm">枚举类-缩略图的样式</param>
        /// <param name="thumbnailName">缩略图文件名</param>
        /// <returns>缩略图数据</returns>
        public static byte[] CreateThumbnail(byte[] sourcedata, int t_width, int t_height, string filename, ThumbModel tm, out string thumbnailName)
        {
            Image thumbnail_image = null;
            Image original_image = null;
            Bitmap final_image = null;
            Graphics graphic = null;
            thumbnailName = Path.GetFileNameWithoutExtension(filename) + ".jpg";

            Stream stream = new MemoryStream(sourcedata);
            original_image = System.Drawing.Image.FromStream(stream);
            // Calculate the new width and height
            int original_paste_x = 0;
            int original_paste_y = 0;
            int original_width = original_image.Width;//截取原图宽度
            int original_height = original_image.Height;//截取原图高度
            int target_paste_x = 0;
            int target_paste_y = 0;
            int target_width1 = t_width;
            int target_height1 = t_height;
            if (tm == ThumbModel.NoDeformationAllThumb)
            {
                float target_ratio = (float)t_width / (float)t_height;//缩略图 宽、高的比例
                float original_ratio = (float)original_width / (float)original_height;//原图 宽、高的比例

                if (target_ratio > original_ratio)//宽拉长
                {
                    target_height1 = t_height;
                    target_width1 = (int)Math.Floor(original_ratio * (float)t_height);
                }
                else
                {
                    target_height1 = (int)Math.Floor((float)t_width / original_ratio);
                    target_width1 = t_width;
                }

                target_width1 = target_width1 > t_width ? t_width : target_width1;
                target_height1 = target_height1 > t_height ? t_height : target_height1;
                target_paste_x = (t_width - target_width1) / 2;
                target_paste_y = (t_height - target_height1) / 2;
            }
            else if (tm == ThumbModel.NoDeformationCenterThumb)
            {
                float target_ratio = (float)t_width / (float)t_height;//缩略图 宽、高的比例
                float original_ratio = (float)original_width / (float)original_height;//原图 宽、高的比例

                if (target_ratio > original_ratio)//宽拉长
                {
                    original_height = (int)Math.Floor((float)original_width / target_ratio);
                }
                else
                {
                    original_width = (int)Math.Floor((float)original_height * target_ratio);
                }
                original_paste_x = (original_image.Width - original_width) / 2;
                original_paste_y = (original_image.Height - original_height) / 2;
            }
            else if (tm == ThumbModel.NoDeformationCenterBig)
            {
                original_paste_x = (original_width - target_width1) / 2;
                original_paste_y = (original_height - target_height1) / 2;
                if (original_height > target_height1) original_height = target_height1;
                if (original_width > target_width1) original_width = target_width1;
            }

            final_image = new System.Drawing.Bitmap(t_width, t_height);
            graphic = System.Drawing.Graphics.FromImage(final_image);
            try
            {
                // graphic.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.White), new System.Drawing.Rectangle(0, 0, t_width, t_height));//背景颜色

                graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High; /* new way */
                graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphic.Clear(Color.White);//背景
                Rectangle SrcRec = new Rectangle(original_paste_x, original_paste_y, original_width, original_height);
                Rectangle targetRec = new Rectangle(target_paste_x, target_paste_y, target_width1, target_height1);
                graphic.DrawImage(original_image, targetRec, SrcRec, GraphicsUnit.Pixel);
                //string saveFileName = AppContext.BaseDirectory+ "TempFoler/Thumb/" + fileMD5 + "_small.jpg";
                //using (FileStream fs = new FileStream(saveFileName, FileMode.Create))
                //{

                //    final_image.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                //    ThumbnailFilename = saveFileName;
                //}
                using (MemoryStream ms = new MemoryStream())
                {

                    final_image.Save(ms, ImageFormat.Jpeg);

                    byte[] buffer = new byte[ms.Length];
                    //Image.Save()会改变MemoryStream的Position，需要重新Seek到Begin
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(buffer, 0, buffer.Length);
                    return buffer;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // Clean up
                if (final_image != null)
                    final_image.Dispose();
                if (graphic != null) graphic.Dispose();
                if (original_image != null) original_image.Dispose();
                if (thumbnail_image != null) thumbnail_image.Dispose();
            }


        }

        public static byte[] CreateThumbnail(byte[] sourcedata, FileInfoModel fileinfo, int fileType,string collectionName,out string thumbnailName)
        {
            Image thumbnail_image = null;
            Bitmap original_image = null;
            Bitmap final_image = null;
            MemoryStream ms = new MemoryStream();

            thumbnailName = Path.GetFileNameWithoutExtension(fileinfo.FileName) +"_"+ fileType+ ".jpg";
            string thumbnailFileName = Path.GetFileNameWithoutExtension(fileinfo.FileId) + "_" + fileType + ".jpg";
            string fullRootPath = Path.Combine(Directory.GetCurrentDirectory(), "Thumbnail");
            if (!Directory.Exists(fullRootPath))
            {
                Directory.CreateDirectory(fullRootPath);
            }
            string fullPath = Path.Combine(fullRootPath, fileinfo.UploadDate.ToString("yyyyMMdd"));
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            string cachePath = Path.Combine(fullPath,thumbnailFileName);


            Stream stream = new MemoryStream(sourcedata);
            original_image = new Bitmap(stream);
            // Calculate the new width and height
            double original_width = original_image.Width;//截取原图宽度
            double original_height = original_image.Height;//截取原图高度
            if (fileType == 1)
            {
                long imageQuality = 50L;
                Encoder myEncoder = Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, imageQuality);
                myEncoderParameters.Param[0] = myEncoderParameter;
                ImageCodecInfo myImageCodecInfo = GetEncoderInfo(ImageFormat.Jpeg);

                //Directory.CreateDirectory(cachePath);
                original_image.Save(cachePath, myImageCodecInfo, myEncoderParameters);
                original_image.Save(ms, myImageCodecInfo, myEncoderParameters);
            }
            else if (fileType == 2) {
                //聊天图片 标准4:3 或者3:4   超过4:2居中截图
                double multiple_x = 2;//横缩放截图阈值
                double multiple_y = 0.5;//竖缩放截图阈值
                //方向
                Boolean orientation = (original_width / original_height)>1?true:false;
                int newWidth = 0;
                int newHeight = 0;
                if (orientation)
                {
                    //横方向
                    if ((original_width / original_height) > multiple_x)
                    {
                        //截图
                        if (original_height <= 300)
                        {
                            newWidth = (int)(original_height / 0.75);
                            newHeight = (int)original_height;
                        }
                        else {
                            double multiple = Math.Round(original_height / 300, 2);
                            newWidth = (int)(original_height / 0.75/multiple);
                            newHeight = 300;
                        }
                        ms = zoomBitmap(original_image, cachePath, newWidth, newHeight);
                    }
                    else {
                        //小于400原图返回
                        if (original_width <= 400)
                        {
                            ms = GetMultipleStream(original_image, cachePath, original_width, original_height, 1);
                        }
                        else {
                             double multiple = Math.Round(original_width / 400, 2);
                             ms = GetMultipleStream(original_image, cachePath, original_width, original_height, multiple);
                        }
                    }
                }
                else {
                    if ((original_width / original_height)< multiple_y)
                    {
                        //截图
                        if (original_width <= 300)
                        {
                            newWidth = (int)original_width;
                            newHeight =(int)(original_width / 0.75);
                        }
                        else
                        {
                            double multiple = Math.Round(original_width / 300, 2);
                            newWidth = 300;
                            newHeight = (int)(original_width / 0.75 / multiple); ;
                        }
                        ms = zoomBitmap(original_image, cachePath, newWidth, newHeight);
                    }
                    else
                    {
                        //小于400原图返回
                        if (original_height <= 400)
                        {
                            ms = GetMultipleStream(original_image, cachePath, original_width, original_height, 1);
                        }
                        else {
                            double multiple = Math.Round(original_height / 400, 2);
                            ms = GetMultipleStream(original_image, cachePath, original_width, original_height, multiple);
                        }
                    }
                }
            }
            else if (fileType == 3)
            {
                double final_width = 160;
                //缩略比例
                double multiple = 1;
                //头像标准160*160
                if (original_width > 160)
                {
                    multiple = Math.Round(original_width / final_width, 2);
                }
                ms=GetMultipleStream(original_image,cachePath,original_width,original_height,multiple);
            }

            try
            {
                byte[] buffer = new byte[ms.Length];
                //Image.Save()会改变MemoryStream的Position，需要重新Seek到Begin
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer, 0, buffer.Length);
                return buffer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // Clean up
                if (final_image != null)
                    final_image.Dispose();
                if (original_image != null) original_image.Dispose();
                if (thumbnail_image != null) thumbnail_image.Dispose();
                if (ms != null) ms.Dispose();
            }
        }
        #endregion

        //// <summary>
        /// 按缩放比返回
        /// </summary>
        private static MemoryStream GetMultipleStream(Bitmap original_image, string cachePath, double original_width, double original_height, double multiple)
        {
            MemoryStream ms = new MemoryStream();
            double final_width = (int)(original_width / multiple);
            double final_height = (int)(original_height / multiple);
            Bitmap newImage = new Bitmap((int)final_width, (int)(final_height));
            using (Graphics graphic = Graphics.FromImage(newImage)) {
                graphic.DrawImage(original_image, 0, 0, (int)final_width, (int)final_height);
                newImage.Save(cachePath, ImageFormat.Jpeg);
                newImage.Save(ms, ImageFormat.Jpeg);
            }
            return ms;
        }

        //// <summary>
        /// 获取图片编码信息
        /// </summary>
        private static ImageCodecInfo GetEncoderInfo(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        /** 
     * 按宽/高缩放图片到指定大小并进行裁剪得到中间部分图片 <br> 
     * 方 法 名：zoomBitmap <br> 
     * @param original_image 源bitmap 
     * @param w 缩放后指定的宽度 
     * @param h 缩放后指定的高度 
     * @return 缩放后的中间部分图片 Bitmap 
     */
        public static MemoryStream zoomBitmap(Bitmap original_image,string cachePath, int w, int h)
        {
            MemoryStream ms = new MemoryStream();
            int x = 0;
            int y = 0;
            int ow = original_image.Width;
            int oh = original_image.Height;
            if ((double)original_image.Width / (double)original_image.Height > (double)w / (double)h)
            {
                oh = original_image.Height;
                ow = original_image.Height * w / h;
                y = 0;
                x = (original_image.Width - ow) / 2;
            }
            else
            {
                ow = original_image.Width;
                oh = original_image.Width * h / w;
                x = 0;
                y = (original_image.Height - oh) / 2;
            }
            try
            {
                Bitmap newImage = new Bitmap(w,h);
                using (Graphics graphic = Graphics.FromImage(newImage))
                {
                    graphic.Clear(Color.Transparent);
                    graphic.DrawImage(original_image, new Rectangle(0, 0, w, h),
                                  new Rectangle(x, y, ow, oh),
                                  GraphicsUnit.Pixel);
                    newImage.Save(cachePath, ImageFormat.Jpeg);
                    newImage.Save(ms, ImageFormat.Jpeg);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.Message);
                return null;
            }
            return ms;
        }

        /// <summary>
        /// Convert Image to Byte[]
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static byte[] ImageToBytes(String sourceFile)
        {
            Bitmap sourceImage = new Bitmap(sourceFile);
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    sourceImage.Save(ms, ImageFormat.Jpeg);
                    byte[] buffer = new byte[ms.Length];
                    //Image.Save()会改变MemoryStream的Position，需要重新Seek到Begin
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(buffer, 0, buffer.Length);
                    return buffer;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // Clean up
                if (sourceImage != null)
                    sourceImage.Dispose();
            }
        }
    }

    public enum ThumbModel
    {
        /// <summary>
        /// 不变形，全部（缩略图）
        /// </summary>
        NoDeformationAllThumb,
        /// <summary>
        /// 变形，全部填充（缩略图）
        /// </summary>
        DeformationAllThumb,
        /// <summary>
        /// 不变形，截中间（缩略图）
        /// </summary>
        NoDeformationCenterThumb,
        /// <summary>
        /// 不变形，截中间（非缩略图）
        /// </summary>
        NoDeformationCenterBig
    }
}
