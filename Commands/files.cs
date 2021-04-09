using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace EzMojiBot.Commands
{
    public class files : BaseCommandModule
    {
                [Command("emoji")]
                [Description("Creates server emoji from image (.jpg, .png) attachment.")]
                [RequireBotPermissions(DSharpPlus.Permissions.ManageEmojis)]
                public async Task Emoji(CommandContext ctx, [Description("Name of new emoji")] string ename="")
                {
                    try
                    {
                        string url = ctx.Message.Attachments.FirstOrDefault().Url;
                        string fname = ctx.Message.Attachments.FirstOrDefault().FileName;
                        int height = ctx.Message.Attachments.FirstOrDefault().Height;
                        int width = ctx.Message.Attachments.FirstOrDefault().Width;
                        
                        int scale = 0;
                        if (height > width)
                        {
                            scale = height / 96;
                        }
                        else if (width > height || width == height)
                        {
                            scale = width / 96;
                        }
                        height /= scale;
                        width /= scale;

                        using (WebClient webClient = new WebClient())
                        {
                            byte[] data = webClient.DownloadData(url);

                            using (MemoryStream mem = new MemoryStream(data))
                            {
                                using (var yourImage = Image.FromStream(mem))
                                {
                                    await MakeEmoji(yourImage, width, height);
                                }
                            }
                        }

                        async Task<Bitmap> MakeEmoji(Image image, int width, int height)
                        {
                            var destRect = new Rectangle(0, 0, width, height);
                            var destImage = new Bitmap(width, height);

                            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                            using (var graphics = Graphics.FromImage(destImage))
                            {
                                graphics.CompositingMode = CompositingMode.SourceCopy;
                                graphics.CompositingQuality = CompositingQuality.HighQuality;
                                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                graphics.SmoothingMode = SmoothingMode.HighQuality;
                                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                                using (var wrapMode = new ImageAttributes())
                                {
                                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                                }
                            }
                            destImage.Save("done.png", ImageFormat.Png);
                            FileStream efile = new FileStream("done.png", FileMode.Open);
                            await ctx.Guild.CreateEmojiAsync(ename, efile).ConfigureAwait(false);
                            return destImage;
                        }
                        File.Delete(@"done.png");

                        await ctx.Message.DeleteAsync().ConfigureAwait(false);
                        await ctx.Channel.SendMessageAsync(ctx.Member.Mention+$@" Emoji :{ename}: has been created!").ConfigureAwait(false);
                    }
                    catch
                    {
                        await ctx.Channel.SendMessageAsync("Failed to create a new emoji!").ConfigureAwait(false);
                    }
                }
                
        private byte[] DownloadData(object url)
        {
            throw new Exception("Failed to create a new emoji!");
        }
    }
}