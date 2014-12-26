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

namespace avcaptureframes
{
    class SaveView : UIViewController
    {
        public SaveView()
            : base() { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Cancelボタンの作成
            UIButton buttonCancel = CreateButton("Cancel", "button_cancel.png", 0);
            buttonCancel.TouchUpInside += (object sender, EventArgs e) =>
            {
                AppDelegate.NaviController.PopToRootViewController(true);
            };

            // Saveボタンの作成
            UIButton buttonSave = CreateButton("Save", "button_save.png", 1);
            buttonSave.TouchUpInside += (object sender, EventArgs e) =>
            {
                CameraPreview.ImageComposite.SaveToPhotosAlbum(  
                    new UIImage.SaveStatus(
                        delegate(UIImage img, NSError error)
                        {
                            var hasError = (error != null);
                            if (hasError)
                            {
                                DiaplayAlert("保存に失敗しました");
                            }
                            else
                            {
                                DiaplayAlert("保存しました");
                            }
                        }));
            };

            // Image表示用のViewの作成
            UIImageView imageView = new UIImageView(new RectangleF(0, 0, 480, 360));
            imageView.Image = CameraPreview.ImageComposite;
            imageView.Transform = CGAffineTransform.MakeRotation((float)Math.PI / 2);

            float scaleWidth = UIScreen.MainScreen.Bounds.Width / (float)360;
            float scaleHeight = UIScreen.MainScreen.Bounds.Height / (float)480;
            float scale = Math.Min(scaleWidth, scaleHeight);
            //// フレームサイズを変更
            imageView.Frame = new RectangleF(0, 0, CameraPreview.size.Height * scale, CameraPreview.size.Width * scale);
            //// 表示位置センターを修正
            imageView.Center = new PointF(UIScreen.MainScreen.Bounds.Width / 2, UIScreen.MainScreen.Bounds.Height / 2);

            // サブビューに画像のビューを追加
            this.View.AddSubview(buttonCancel);
            this.View.AddSubview(buttonSave);


            this.View.AddSubview(imageView);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
        }

        UIButton CreateButton(string title, string imageName, int id)
        {
            UIButton button = new UIButton();
            UIImage image = UIImage.FromBundle(imageName);
            button.SetTitle(title, UIControlState.Normal);
            button.SetBackgroundImage(image, UIControlState.Normal);
            float height = UIScreen.MainScreen.Bounds.Width / (float)4;
            float width = UIScreen.MainScreen.Bounds.Width / 2;
            button.Frame = new RectangleF(width * id, UIScreen.MainScreen.Bounds.Height - height, width, height);

            return button;
        }

        void DiaplayAlert(string title)
        {
            var alert = new UIAlertView(title, "", null, "OK");
            alert.Show();
            alert.Clicked += (object sen, UIButtonEventArgs eve) =>
            {
                AppDelegate.NaviController.PopToRootViewController(true);
            };
        }
    }
}
