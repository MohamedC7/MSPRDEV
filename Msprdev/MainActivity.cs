using System;
using System.Web;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Android;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Navigation;
using Google.Android.Material.Snackbar;


using System.Text;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Drawing;
using System.Drawing.Imaging;
using Xamarin.Forms;
using static Android.Graphics.ImageDecoder;
using View = Android.Views.View;
using Bitmap = System.Drawing.Bitmap;
using Image = Xamarin.Forms.Image;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.Android;
using Android.Util;



//using IronPython.Hosting;
//using Microsoft.Scripting.Hosting;

namespace Msprdev
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        Android.Widget.Button captureButton;
        Android.Widget.Button captureNavButton;
        Android.Widget.Button uploadButton;
        Android.Widget.Button uploadNavButton;
        Android.Widget.ImageView thisImageView;

        readonly string[] permissionGroup =
        {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera
        };
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            captureButton = (Android.Widget.Button)FindViewById(Resource.Id.captureButton);
            captureNavButton = (Android.Widget.Button)FindViewById(Resource.Id.nav_camera);
            
            uploadButton = (Android.Widget.Button)FindViewById(Resource.Id.uploadButton);
            uploadNavButton = (Android.Widget.Button)FindViewById(Resource.Id.nav_gallery);

            thisImageView = (Android.Widget.ImageView)FindViewById(Resource.Id.thisImageView);

            captureButton.Click += CaptureButton_Click;
            //captureNavButton.Click += CaptureButton_Click;

            uploadButton.Click += UploadButton_Click;
            //uploadNavButton.Click += UploadButton_Click;
            RequestPermissions(permissionGroup, 0);
        }

        private void UploadButton_Click(object sender, System.EventArgs e)
        {
            UploadPhoto();
        }

        private void CaptureButton_Click(object sender, System.EventArgs e)
        {

            TakePhoto();
            
        }

        async void TakePhoto()
        {
           
            await Plugin.Media.CrossMedia.Current.Initialize();

            var file = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                CompressionQuality = 40,
                Name = "myimage.jpg",
                Directory = "sample"

            });

            if (file == null)
            {
                return;
            }

            // Convert file to byte array and set the resulting bitmap to imageview
            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);
            Android.Graphics.Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
            thisImageView.SetImageBitmap(bitmap);
            Console.WriteLine(Compare(bitmap, bitmap));

            ImageProcessing(bitmap);
            
        }


        public static Android.Graphics.Bitmap ImageProcessing(Android.Graphics.Bitmap source)
        {
            
            try
            {

                DirectoryInfo dir = new DirectoryInfo(@"D:\MSPRDEV\Msprdev\model\2D\");
                FileInfo[] imageFiles = dir.GetFiles("*.png");
                Console.WriteLine("Found {0} *.png files\n", imageFiles.Length);
                bool found = false;

                foreach (FileInfo img in imageFiles)
                {
                    found = IronPython1(img.ToString(), source.ToString());
                    if (found)
                    {
                        dir = new DirectoryInfo(@"D:\MSPRDEV\Msprdev\model\3D\");
                        imageFiles = dir.GetFiles(img + "*3D.png");
                        // Get the 3D model for the 2D model
                        
                        break;
                    }

                }

                // detect the  color 
                var current3D = imageFiles[0];
                System.Drawing.Color colorofobject = IronPython2(current3D.ToString());

                // set color to the 3D model
                Android.Graphics.Bitmap bitmap = IronPython3(current3D.ToString(), colorofobject);
                return bitmap;              

            }



            catch (Exception exp)
            {

            }
           
            // return the 3D image in the view
            return source;


        }
        static string PathAddBackslash(string path)
        {
            // They're always one character but EndsWith is shorter than
            // array style access to last path character. Change this
            // if performance are a (measured) issue.
            string separator1 = System.IO.Path.DirectorySeparatorChar.ToString();
            string separator2 = System.IO.Path.AltDirectorySeparatorChar.ToString();

            // Trailing white spaces are always ignored but folders may have
            // leading spaces. It's unusual but it may happen. If it's an issue
            // then just replace TrimEnd() with Trim(). Tnx Paul Groke to point this out.
            path = path.TrimEnd();

            // Argument is always a directory name then if there is one
            // of allowed separators then I have nothing to do.
            if (path.EndsWith(separator1) || path.EndsWith(separator2))
                return path;

            // If there is the "alt" separator then I add a trailing one.
            // Note that URI format (file://drive:\path\filename.ext) is
            // not supported in most .NET I/O functions then we don't support it
            // here too. If you have to then simply revert this check:
            // if (path.Contains(separator1))
            //     return path + separator1;
            //
            // return path + separator2;
            if (path.Contains(separator2))
                return path + separator2;

            // If there is not an "alt" separator I add a "normal" one.
            // It means path may be with normal one or it has not any separator
            // (for example if it's just a directory name). In this case I
            // default to normal as users expect.
            return path + separator1;
        }
        public static bool Compare(Android.Graphics.Bitmap img1, Android.Graphics.Bitmap img2)
        {
            string img1_ref, img2_ref;
            bool equal = true;
            int count1 =1;
            int count2 = 0;
            bool flag = true;

            if (img1.Width == img2.Width && img1.Height == img2.Height)
            {
                for (int i = 0; i < img1.Width; i++)
                {
                    for (int j = 0; j < img1.Height; j++)
                    {
                        img1_ref = img1.GetPixel(i, j).ToString();
                        img2_ref = img2.GetPixel(i, j).ToString();
                        if (img1_ref != img2_ref)
                        {
                            count2++;
                            flag = false;
                            break;
                        }
                        count1++;
                    }

                }
                if (flag == false)
                    Console.WriteLine("Sorry, Images are not same , " + count2 + " wrong pixels found");
                else
                    Console.WriteLine(" Images are same , " + count1 + " same pixels found and " + count2 + " wrong pixels found");
            }
            else
                Console.WriteLine("can not compare this images");
        return flag;
                    }



        static bool IronPython1(string img1, string img2)
        {                    
              //instance of python engine
              var engine = Python.CreateEngine();
              //reading code from file
              var source = engine.CreateScriptSourceFromFile(System.IO.Path.Combine("D:\\MSPRDEV\\Msprdev\\", "check_if_two_images_are_equal.py"));
              var scope = engine.CreateScope();
              //executing script in scope
              source.Execute(scope);
              var classImageProcess = scope.GetVariable("ImageProcess");
              //initializing class
              var ImageProcessInstance = engine.Operations.CreateInstance(classImageProcess);
              Console.WriteLine("From Iron Python");
              return ImageProcessInstance.equal(img1, img2);

        }

        static System.Drawing.Color IronPython2(string img)
        {
            //instance of python engine
            var engine = Python.CreateEngine();
            //reading code from file
            var source = engine.CreateScriptSourceFromFile(System.IO.Path.Combine("D:\\MSPRDEV\\Msprdev\\", "detect_color.py"));
            var scope = engine.CreateScope();
            //executing script in scope
            source.Execute(scope);
            var DetectColor = scope.GetVariable("DetectColor");
            //initializing class
            var ImageProcessInstance = engine.Operations.CreateInstance(DetectColor);
            Console.WriteLine("From Iron Python");
            return ImageProcessInstance.detectcolor(img);

        }

        static Android.Graphics.Bitmap IronPython3(string objectC, System.Drawing.Color color)
        {
            //instance of python engine
            var engine = Python.CreateEngine();
            //reading code from file
            var source = engine.CreateScriptSourceFromFile(System.IO.Path.Combine("D:\\MSPRDEV\\Msprdev\\", "color_3D_model.py"));
            var scope = engine.CreateScope();
            //executing script in scope
            source.Execute(scope);
            var SetColor = scope.GetVariable("Color3D");
            //initializing class
            var ImageProcessInstance = engine.Operations.CreateInstance(SetColor);
            Console.WriteLine("From Iron Python");
            return ImageProcessInstance.setColor(objectC, color);

        }


        async void UploadPhoto()
        {
            await Plugin.Media.CrossMedia.Current.Initialize();

            if (!Plugin.Media.CrossMedia.Current.IsPickPhotoSupported)
            {
                Android.Widget.Toast.MakeText(this, "Upload not supported on this device", Android.Widget.ToastLength.Short).Show();
                return;
            }

            var file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Full,
                CompressionQuality = 40

            });

            // Convert file to byre array, to bitmap and set it to our ImageView

            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);
            Android.Graphics.Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
            thisImageView.SetImageBitmap(bitmap);

        }

        
        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if(drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Share on social media", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.nav_camera)
            {
                // Handle the camera action
            }
            else if (id == Resource.Id.nav_gallery)
            {

            }
            else if (id == Resource.Id.nav_slideshow)
            {

            }
            else if (id == Resource.Id.nav_manage)
            {

            }
            else if (id == Resource.Id.nav_share)
            {

            }
            else if (id == Resource.Id.nav_send)
            {

            }

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }





        


        public string Runcmd(string cmd, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "C:\\Users\\mchaieb\\AppData\\Local\\Programs\\Python\\Python39\\python.exe";
            start.Arguments = string.Format("\"{0}\" \"{1}\"", cmd, args);
            start.UseShellExecute = false;// Do not use OS shell
            start.CreateNoWindow = true; // We don't need new window
            start.RedirectStandardOutput = true;// Any output, generated by application will be redirected back
            start.RedirectStandardError = true; // Any error in standard output will be redirected back (for example exceptions)
            using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string stderr = process.StandardError.ReadToEnd(); // Here are the exceptions from our Python script
                    string result = reader.ReadToEnd(); // Here is the result of StdOut(for example: print "test")
                    return result;
                }
            }
        }
    }
   
}

