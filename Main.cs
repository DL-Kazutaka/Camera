//
// How to capture video frames from the camera as images using AVFoundation sample
//
// Based on Apple's technical Q&A QA1702 sample
//
//
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
	public class Application
	{
		static void Main (string[] args)
		{
			UIApplication.Main (args);
		}
	}

	public partial class AppDelegate : UIApplicationDelegate
	{
        public static UINavigationController NaviController;
        //public static UIImageView ImageView;
        //public static UIImageView ImageViewFrame;
        //public static UIImage ImageFrame;
        //public static UIButton ButtonTake;
        //UIViewController vc;
		
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
            UIApplication.SharedApplication.SetStatusBarHidden(true, false);
            // カメラ画像用のViewを作成、サイズは何でも良い
            //ImageView = new UIImageView (new RectangleF (0, 0, 480, 360));
            //ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            //ImageView.Center = new PointF(UIScreen.MainScreen.Bounds.Width / 2, UIScreen.MainScreen.Bounds.Height / 2);

            //ImageViewFrame = new UIImageView(new RectangleF(0, 0, 480, 360));
            //ImageViewFrame.ContentMode = UIViewContentMode.ScaleAspectFit;
            //ImageViewFrame.Image = UIImage.FromBundle("waku1.png");
            ////ImageViewFrame.Center = new PointF(UIScreen.MainScreen.Bounds.Width / 2, UIScreen.MainScreen.Bounds.Height / 2);

            ////ImageView.AddSubview(ImageViewFrame);

            //// フレーム用
            //ImageFrame = UIImage.FromBundle("waku1.png");

            //// ボタン
            //ButtonTake = UIButton.FromType(UIButtonType.RoundedRect);
            //UIImage button = UIImage.FromBundle("button_blue.png");
            //ButtonTake.SetBackgroundImage(button, UIControlState.Normal);
            //float height = UIScreen.MainScreen.Bounds.Width / (float)4;
            //ButtonTake.Frame = new RectangleF(height * 3 / 2, UIScreen.MainScreen.Bounds.Height - height, height, height);
            //ButtonTake.ContentMode = UIViewContentMode.ScaleAspectFit;
            //ButtonTake.SetTitle("Take", UIControlState.Normal);
            //ButtonTake.TouchUpInside += (object sender, EventArgs e) =>
            //{
            //    AppDelegate.NaviController.PushViewController(new SaveView(), false);
            //};

            //// ルートのビューコントローラーを作成
            ////vc = new UIViewController();
            //vc = new CameraPreview();


            //// フレーム画像名
            //string[] images = {
            //                      "waku1.png",
            //                      "waku2.png",
            //                      "waku3.png",
            //                      "waku4.png",
            //                  };
            //// フレーム画像選択用のボタンを作成
            //for (int i = 0; i < images.Length; i++)
            //    vc.View.AddSubview(CreateButton(images[i], i, images.Length));

            
            //// サブビューに作成したビューを追加
            //vc.View.AddSubview(ImageView);
            //vc.View.AddSubview(ImageViewFrame);
            //vc.View.AddSubview(ButtonTake);

            window = new UIWindow(UIScreen.MainScreen.Bounds);
            // ルートのビューコントローラーに作成したコントローラーを追加
			//window.RootViewController = vc;
            NaviController = new UINavigationController(new CameraPreview());
            NaviController.SetNavigationBarHidden(true, false);
            window.RootViewController = NaviController;

            // ウィンドウを表示
			window.MakeKeyAndVisible ();
			window.BackgroundColor = UIColor.Black;

			return true;
		}

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            //ImageFrame.Dispose();
            //ImageView.Image.Dispose();
            //ImageView.Dispose();
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
                    //AppDelegate.ImageFrame = image;
                };

            return button;
        }
	}
}

