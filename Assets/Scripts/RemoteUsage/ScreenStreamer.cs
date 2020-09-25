using UnityEngine;
using System.Collections;
using System.IO;
using Robotsystemcommunication;
 
public class ScreenStreamer : MonoBehaviour
{
    public Camera cameraToStream;

    public volatile int captureWidth = 1080;
    public volatile int captureHeight = 1080;


    public volatile ImageType imageType = ImageType.Jpg;
    public volatile int jpegQuality = 75;

    // folder to write output (defaults to data path)
    public string folder;

    [HideInInspector]
    // The variable holds the latest screen capture. It is set 
    // volatile to allow thread safe reading from the gRPC server's
    // thread and writing in this main thread
    public volatile byte[] latestScreenCapture;

    private Rect rect;
    private RenderTexture renderTexture;
    private Texture2D screenShot;
    private int counter = 0; // image #

    private bool captureScreenshot = false;

    // create a unique filename using a one-up variable
    private string uniqueFilename(int width, int height, ImageType imageType)
    {
        // if folder not specified by now use a good default
        if (folder == null || folder.Length == 0)
        {
            folder = Application.dataPath;
            if (Application.isEditor)
            {
                // put screenshots in folder above asset path so unity doesn't index the files
                var stringPath = folder + "/..";
                folder = Path.GetFullPath(stringPath);
            }
            folder += "/screenshots";

            // make sure directoroy exists
            System.IO.Directory.CreateDirectory(folder);

            // count number of files of specified format in folder
            string mask = string.Format("screen_{0}x{1}*.{2}", width, height, imageType.ToString().ToLower());
            counter = Directory.GetFiles(folder, mask, SearchOption.TopDirectoryOnly).Length;
        }

        // use width, height, and counter for unique file name
        var filename = string.Format("{0}/screen_{1}x{2}_{3}.{4}", folder, width, height, counter, imageType.ToString().ToLower());

        // up counter for next call
        ++counter;

        // return unique filename
        return filename;
    }

    // Use keypresses for testing screencapture otherwise 
    // latest screencapture is saved to the variable
    void Update()
    {
        // check keyboard 'k' for one time screenshot capture
        captureScreenshot |= Input.GetKeyDown("k");

        if (captureScreenshot)
        {
            TakeScreenShot(captureWidth, captureHeight);

            SaveToFile(imageType, jpegQuality);
        }
        else
        {
            latestScreenCapture = GetScreenCapture(captureWidth, captureHeight, imageType, jpegQuality);
        }
    }

    public byte[] GetScreenCapture(int captureWidth, int captureHeight, ImageType imageType, int jpegQuality=75)
    {
        TakeScreenShot(captureWidth, captureHeight);
        byte[] rawImage;
        if (imageType == ImageType.Jpg)
        {
            rawImage = screenShot.EncodeToJPG(jpegQuality);
        }
        else
        {
            rawImage = screenShot.EncodeToPNG();
        }
        return rawImage;
    }

    void TakeScreenShot(int captureWidth, int captureHeight)
    {
        // create screenshot objects if needed
        if (renderTexture == null)
        {
            // creates off-screen render texture that can rendered into
            rect = new Rect(0, 0, captureWidth, captureHeight);
            renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
            screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
        }
        cameraToStream.targetTexture = renderTexture;
        cameraToStream.Render();
        // read pixels will read from the currently active render texture so make our offscreen 
        // render texture active and then read the pixels
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(rect, 0, 0);
        // reset active camera texture and render texture
        cameraToStream.targetTexture = null;
        RenderTexture.active = null;
    }

    void SaveToFile(ImageType imageType, int jpegQuality=75)
    {
        // get our unique filename
        string filename = uniqueFilename((int) rect.width, (int) rect.height, imageType);

        // pull in our file header/data bytes for the specified image format (has to be done from main thread)
        byte[] fileHeader = null;
        byte[] fileData = null;
        if (imageType == ImageType.Png)
        {
            fileData = screenShot.EncodeToPNG();
        }
        else if (imageType == ImageType.Jpg)
        {
            fileData = screenShot.EncodeToJPG(jpegQuality);
        }

        // create new thread to save the image to file (only operation that can be done in background)
        new System.Threading.Thread(() =>
        {
            // create file and write optional header with image bytes
            var f = System.IO.File.Create(filename);
            if (fileHeader != null) f.Write(fileHeader, 0, fileHeader.Length);
            f.Write(fileData, 0, fileData.Length);
            f.Close();
            Debug.Log(string.Format("Wrote screenshot {0} of size {1}", filename, fileData.Length));
        }).Start();
    }
}