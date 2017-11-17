using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

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
            catch(Exception ex)
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
        #endregion



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
