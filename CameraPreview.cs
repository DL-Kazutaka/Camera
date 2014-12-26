using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.AVFoundation;
using MonoTouch.CoreVideo;
using MonoTouch.CoreMedia;
using MonoTouch.CoreGraphics;

using MonoTouch.CoreFoundation;
using System.Runtime.InteropServices;

namespace avcaptureframes
{
    class CameraPreview : UIViewController
    {
        AVCaptureSession session;
        DispatchQueue queue;
        OutputRecorder outputRecorder;
        AVAudioPlayer audioPlayer;

        public static UIImageView ImageView;
        public static UIImageView ImageViewFrame;
        public static UIImage ImageComposite;
        public static SizeF size;

        public static UIButton ButtonTake;
        
        public override void ViewDidLoad()
        {
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            SetupCaptureSession();

            audioPlayer = AVAudioPlayer.FromUrl(new NSUrl("bull.wav"));

            CreateLayout();
        }

        void CreateLayout()
        {
            // カメラ画像用のViewを作成、サイズは何でも良い
            ImageView = new UIImageView(new RectangleF(0, 0, 480, 360));
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            ImageView.Center = new PointF(UIScreen.MainScreen.Bounds.Width / 2, UIScreen.MainScreen.Bounds.Height / 2);

            ImageViewFrame = new UIImageView(new RectangleF(0, 0, 480, 360));
            ImageViewFrame.ContentMode = UIViewContentMode.ScaleAspectFit;
            ImageViewFrame.Image = UIImage.FromBundle("waku1.png");
            ImageViewFrame.Center = new PointF(UIScreen.MainScreen.Bounds.Width / 2, UIScreen.MainScreen.Bounds.Height / 2);

            // ボタン
            ButtonTake = UIButton.FromType(UIButtonType.RoundedRect);
            UIImage button = UIImage.FromBundle("button_blue.png");
            ButtonTake.SetBackgroundImage(button, UIControlState.Normal);
            float height = UIScreen.MainScreen.Bounds.Width / (float)4;
            ButtonTake.Frame = new RectangleF(height * 3 / 2, UIScreen.MainScreen.Bounds.Height - height, height, height);
            ButtonTake.ContentMode = UIViewContentMode.ScaleAspectFit;
            ButtonTake.SetTitle("Take", UIControlState.Normal);
            ButtonTake.TouchUpInside += (object sender, EventArgs e) =>
            {
                audioPlayer.Play();
                CreateComposite();
                AppDelegate.NaviController.PushViewController(new SaveView(), false);
            };

            // フレーム画像名
            string[] images = {
                                  "waku1.png",
                                  "waku2.png",
                                  "waku3.png",
                                  "waku4.png",
                              };

            // フレーム画像選択用のボタンを作成
            for (int i = 0; i < images.Length; i++)
                this.View.AddSubview(CreateButton(images[i], i, images.Length));

            // サブビューに作成したビューを追加
            this.View.AddSubview(ImageView);
            this.View.AddSubview(ImageViewFrame);
            this.View.AddSubview(ButtonTake);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            //audioPlayer.Play();

            session.StopRunning();
            session.Dispose();
            session = null;

            outputRecorder.Dispose();
            outputRecorder = null;
        }

        /// <summary>
        /// キャプチャフレーム選択用のボタンを作成
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="id"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        UIButton CreateButton(string imageName, int id, int total)
        {
            float width = UIScreen.MainScreen.Bounds.Width / (float)total;
            UIButton button = new UIButton();
            UIImage image = UIImage.FromBundle(imageName);
            button.SetBackgroundImage(image, UIControlState.Normal);
            button.Frame = new RectangleF(width * id, 0, width, width);
            button.BackgroundColor = UIColor.Blue;
            button.TouchUpInside += (object sender, EventArgs e) =>
            {
                ImageViewFrame.Image = image;
            };

            return button;
        }

        /// <summary>
        /// 合成画像の作成
        /// </summary>
        void CreateComposite()
        {
            // ビットマップ形式のグラフィックスコンテキストの生成
            UIGraphics.BeginImageContextWithOptions(size, false, UIScreen.MainScreen.Scale);
            // 領域を決めて塗りつぶす
            CameraPreview.ImageView.Image.DrawAsPatternInRect(new RectangleF(0, 0, size.Width, size.Height));
            // フレーム画像を取得
            // 領域を決めて塗りつぶす(画像サイズが塗りつぶす領域に満たない場合はループする)
            CameraPreview.ImageViewFrame.Image.DrawAsPatternInRect(new RectangleF(0, 0, size.Width, size.Height));
            // 現在のグラフィックスコンテキストの画像を取得する
            var compositeImage = UIGraphics.GetImageFromCurrentImageContext();
            // 現在のグラフィックスコンテキストへの編集を終了
            // (スタックの先頭から削除する)
            UIGraphics.EndImageContext();

            // イメージを保存
            ImageComposite = compositeImage;
        }

        /// <summary>
        /// セッションの設定
        /// </summary>
        /// <returns></returns>
        bool SetupCaptureSession()
        {
            // configure the capture session for low resolution, change this if your code
            // can cope with more data or volume
            session = new AVCaptureSession()
            {
                SessionPreset = AVCaptureSession.PresetMedium
            };

            // create a device input and attach it to the session
            var captureDevice = AVCaptureDevice.DefaultDeviceWithMediaType(AVMediaType.Video);
            if (captureDevice == null)
            {
                Console.WriteLine("No captureDevice - this won't work on the simulator, try a physical device");
                return false;
            }
            //Configure for 15 FPS. Note use of LockForConigfuration()/UnlockForConfiguration()
            NSError error = null;
            captureDevice.LockForConfiguration(out error);
            if (error != null)
            {
                Console.WriteLine(error);
                captureDevice.UnlockForConfiguration();
                return false;
            }
            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                captureDevice.ActiveVideoMinFrameDuration = new CMTime(1, 15);
            captureDevice.UnlockForConfiguration();


            var input = AVCaptureDeviceInput.FromDevice(captureDevice);
            if (input == null)
            {
                Console.WriteLine("No input - this won't work on the simulator, try a physical device");
                return false;
            }
            session.AddInput(input);

            // create a VideoDataOutput and add it to the sesion
            var output = new AVCaptureVideoDataOutput()
            {
                VideoSettings = new AVVideoSettings(CVPixelFormatType.CV32BGRA),
            };


            // configure the output
            queue = new MonoTouch.CoreFoundation.DispatchQueue("myQueue");
            outputRecorder = new OutputRecorder();
            output.SetSampleBufferDelegate(outputRecorder, queue);
            session.AddOutput(output);

            session.StartRunning();
            return true;
        }

        public class OutputRecorder : AVCaptureVideoDataOutputSampleBufferDelegate
        {
            public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
            {
                try
                {
                    var image = ImageFromSampleBuffer(sampleBuffer);

                    // Do something with the image, we just stuff it in our main view.
                    CameraPreview.ImageView.BeginInvokeOnMainThread(delegate
                    {
                        //// プレビュー表示中
                        CameraPreview.ImageView.Image = image;
                        CameraPreview.ImageView.Transform = CGAffineTransform.MakeRotation((float)Math.PI / 2);

                        CameraPreview.ImageViewFrame.Transform = CGAffineTransform.MakeRotation((float)Math.PI / 2);

                        ////// ベースのサイズの取得
                        CameraPreview.size = image.Size;
                        var size = image.Size;
                        ////// ビットマップ形式のグラフィックスコンテキストの生成
                        //UIGraphics.BeginImageContextWithOptions(size, false, UIScreen.MainScreen.Scale);
                        ////// 領域を決めて塗りつぶす
                        ////AppDelegate.ImageView.Image.DrawAsPatternInRect(new RectangleF(0, 0, size.Width, size.Height));
                        ////// フレーム画像を取得
                        ////// 領域を決めて塗りつぶす(画像サイズが塗りつぶす領域に満たない場合はループする)
                        ////AppDelegate.ImageFrame.DrawAsPatternInRect(new RectangleF(0, 0, AppDelegate.ImageFrame.Size.Width, AppDelegate.ImageFrame.Size.Height));
                        ////// 現在のグラフィックスコンテキストの画像を取得する
                        //var dstImage = UIGraphics.GetImageFromCurrentImageContext();
                        ////// 現在のグラフィックスコンテキストへの編集を終了
                        ////// (スタックの先頭から削除する)
                        //UIGraphics.EndImageContext();

                        //// 画面に最大限表示されるように、スケールを算出する
                        //float scaleWidth = UIScreen.MainScreen.Bounds.Width / (float)dstImage.Size.Height;
                        //float scaleHeight = UIScreen.MainScreen.Bounds.Height / (float)dstImage.Size.Width;
                        //float scale = Math.Min(scaleWidth, scaleHeight);


                        float scaleWidth = UIScreen.MainScreen.Bounds.Width / (float)image.Size.Height;
                        float scaleHeight = UIScreen.MainScreen.Bounds.Height / (float)image.Size.Width;
                        float scale = Math.Min(scaleWidth, scaleHeight);

                        //// フレームサイズを変更
                        CameraPreview.ImageView.Frame = new RectangleF(0, 0, size.Height * scale, size.Width * scale);
                        //// 表示位置センターを修正
                        CameraPreview.ImageView.Center = new PointF(UIScreen.MainScreen.Bounds.Width / 2, UIScreen.MainScreen.Bounds.Height / 2);

                        //// フレームサイズを変更
                        CameraPreview.ImageViewFrame.Frame = new RectangleF(0, 100, size.Height * scale, size.Width * scale);
                        //// 表示位置センターを修正
                        CameraPreview.ImageViewFrame.Center = new PointF(UIScreen.MainScreen.Bounds.Width / 2, UIScreen.MainScreen.Bounds.Height / 2);

                        ////// 合成画像をImageに設定する
                        ////AppDelegate.ImageView.Image = dstImage;

                        // 画像に合わせてボタンサイズも変更
                        float height = (UIScreen.MainScreen.Bounds.Height - CameraPreview.ImageView.Frame.Height) / 2;
                        CameraPreview.ButtonTake.Frame = new RectangleF((UIScreen.MainScreen.Bounds.Width - height) / 2, UIScreen.MainScreen.Bounds.Height - height, height, height);
                        // 表示位置センターを修正
                        CameraPreview.ButtonTake.Center = new PointF(UIScreen.MainScreen.Bounds.Width / 2, UIScreen.MainScreen.Bounds.Height - height / 2);
                    });

                    //
                    // Although this looks innocent "Oh, he is just optimizing this case away"
                    // this is incredibly important to call on this callback, because the AVFoundation
                    // has a fixed number of buffers and if it runs out of free buffers, it will stop
                    // delivering frames. 
                    //	
                    sampleBuffer.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            UIImage ImageFromSampleBuffer(CMSampleBuffer sampleBuffer)
            {
                // Get the CoreVideo image
                using (var pixelBuffer = sampleBuffer.GetImageBuffer() as CVPixelBuffer)
                {
                    // Lock the base address
                    pixelBuffer.Lock(0);
                    // Get the number of bytes per row for the pixel buffer
                    var baseAddress = pixelBuffer.BaseAddress;
                    int bytesPerRow = pixelBuffer.BytesPerRow;
                    int width = pixelBuffer.Width;
                    int height = pixelBuffer.Height;
                    var flags = CGBitmapFlags.PremultipliedFirst | CGBitmapFlags.ByteOrder32Little;
                    // Create a CGImage on the RGB colorspace from the configured parameter above
                    using (var cs = CGColorSpace.CreateDeviceRGB())
                    using (var context = new CGBitmapContext(baseAddress, width, height, 8, bytesPerRow, cs, (CGImageAlphaInfo)flags))
                    using (var cgImage = context.ToImage())
                    {
                        pixelBuffer.Unlock(0);
                        return UIImage.FromImage(cgImage);
                    }
                }
            }
        }
    }
}
