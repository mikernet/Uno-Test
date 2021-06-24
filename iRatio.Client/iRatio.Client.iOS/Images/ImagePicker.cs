using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;

namespace UnoTest.Client.Images
{
    public class ImagePicker
    {
        TaskCompletionSource<Stream> taskCompletionSource;
        UIImagePickerController imagePicker;

        public Task<Stream> GetImageStreamAsync()
        {
            // Create and define UIImagePickerController
            imagePicker = new UIImagePickerController {
                SourceType = UIImagePickerControllerSourceType.PhotoLibrary,
                MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary)
            };

            // Set event handlers
            imagePicker.FinishedPickingMedia += OnImagePickerFinishedPickingMedia;
            imagePicker.Canceled += OnImagePickerCancelled;

            // Present UIImagePickerController;
            UIWindow window = UIApplication.SharedApplication.KeyWindow;
            var viewController = window.RootViewController;
            viewController.PresentViewController(imagePicker, true, null);

            // Return Task object
            taskCompletionSource = new TaskCompletionSource<Stream>();
            return taskCompletionSource.Task;
        }

        private void OnImagePickerFinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs args)
        {
            UIImage image = args.EditedImage ?? args.OriginalImage;

            if (image != null) {
                int width = (int)(image.CurrentScale * image.Size.Width);
                int height = (int)(image.CurrentScale * image.Size.Height);

                if (width > 1000 || height > 1000) {
                    int newWidth;
                    int newHeight;

                    if (width > height) {
                        newWidth = Math.Min(width, 1000);
                        newHeight = (int)(newWidth * (float)height / width);
                    }
                    else {
                        newHeight = Math.Min(height, 1000);
                        newWidth = (int)(newHeight * (float)width / height);
                    }

                    var oldImage = image;
                    image = oldImage.Scale(new CGSize(newWidth, newHeight), 1);
                    oldImage.Dispose();
                }

                // Convert UIImage to .NET Stream object
                NSData data;
                //if (args.ReferenceUrl.PathExtension.Equals("png", StringComparison.OrdinalIgnoreCase)) {
                //    data = image.AsPNG();
                //}
                //else {
                    data = image.AsJPEG(0.8f);
                //}
                Stream stream = data.AsStream();

                UnregisterEventHandlers();

                // Set the Stream as the completion of the Task
                taskCompletionSource.SetResult(stream);
            }
            else {
                UnregisterEventHandlers();
                taskCompletionSource.SetResult(null);
            }
            imagePicker.DismissModalViewController(true);
        }

        private void OnImagePickerCancelled(object sender, EventArgs args)
        {
            UnregisterEventHandlers();
            taskCompletionSource.SetResult(null);
            imagePicker.DismissModalViewController(true);
        }

        private void UnregisterEventHandlers()
        {
            imagePicker.FinishedPickingMedia -= OnImagePickerFinishedPickingMedia;
            imagePicker.Canceled -= OnImagePickerCancelled;
        }
    }
}