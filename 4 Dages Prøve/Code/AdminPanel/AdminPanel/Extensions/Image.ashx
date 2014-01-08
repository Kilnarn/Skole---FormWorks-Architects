<%@ WebHandler Language="C#" Class="ImageHandler" %>

using System;
using System.Web;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Collections.Generic;

public class ImageHandler : IHttpHandler {
    
    public void ProcessRequest (HttpContext context) {

		if (String.IsNullOrEmpty(context.Request["f"]))
			return;
		
		//Get the image to show
		string file = context.Request["f"];
		
		
		if(String.IsNullOrEmpty(context.Request["w"]) && String.IsNullOrEmpty(context.Request["h"]) && String.IsNullOrEmpty(context.Request["r"]))
			context.Response.Redirect(context.Request["f"]); //If no parameters set, just redirect to original image

		//Initialize width and height variables
		float w = 0f, h = 0f, tmp = 0f, r= 0f;
		bool crop = false, enlarge = false;

		//Get the width and/or height if specified
		if (!String.IsNullOrEmpty(context.Request["w"]))
			w = int.Parse(context.Request["w"]);
		if (!String.IsNullOrEmpty(context.Request["h"]))
			h = int.Parse(context.Request["h"]);
		if (!String.IsNullOrEmpty(context.Request["r"]))
			r = int.Parse(context.Request["r"]);


		string fullpath = context.Request.MapPath(file);
		string cache = context.Server.MapPath("~" + Path.DirectorySeparatorChar + "cache" + Path.DirectorySeparatorChar);
		string cache_name = "";
		
		if(w != 0 || h != 0)
			cache_name = (int)w + "x" + (int)h + "_" + Path.GetFileNameWithoutExtension(fullpath) + "_" + File.GetCreationTimeUtc(fullpath).ToFileTimeUtc() + ".jpg";
		else
			cache_name = Path.GetFileNameWithoutExtension(fullpath) + "_" + File.GetCreationTimeUtc(fullpath).ToFileTimeUtc() + ".jpg";

		if (File.Exists(cache + cache_name))
			context.Response.Redirect("/cache/" + cache_name);				

		try
		{
			//Create a bitmap object			
			Bitmap bm = new Bitmap(context.Request.MapPath(file));

			if (w != 0 && h == 0)							//If only a width is specified 
				h = w / bm.Width * bm.Height;				//Scale height to fit width
			else if (w == 0 && h != 0)						//If only a height is specified 
				w = h / bm.Height * bm.Width;				//Scale width to fit height
			else if (w == 0 && h == 0)						//If neither height or width is specified
			{ w = 150f; h = w / bm.Width * bm.Height; }		//Fix width at 150px and scale width to fit
			else if (w != 0 && h != 0)						//Scale to width and crop at height 'h'
			{
				if (h < w)
				{
					tmp = h;
					h = w / bm.Width * bm.Height;				//Scale height to fit width
					if (h > tmp)
						crop = true;
					else
						enlarge = true;
				}
				else if(h > w)
				{
					tmp = w;
					w = h / bm.Height * bm.Width;				//Scale height to fit 
					if (w > tmp)
						crop = true;
				}
				else if (h == w)
				{
					tmp = h;
					h = w / bm.Width * bm.Height;				//Scale height to fit width
					if (h > tmp)
						crop = true;
					else
						enlarge = true;
				}
					
			}

//            FileInfo info = new FileInfo(context.Request.MapPath(file));
            
			//Check cache
			if (!Directory.Exists(cache))
				Directory.CreateDirectory(cache);

			//Get the thumb and clean up
			Bitmap thumb = ResizeImage(bm, (int)w, (int)h);
			bm.Dispose();

			if (crop)
				thumb = CropImage((Bitmap)thumb, (int)w, (int)tmp); 
			else if(enlarge)
				thumb = EnlargeCanvas((Bitmap)thumb, (int)w, (int)tmp);

			if (r > 0)
				thumb = CreateRoundedCorners((Bitmap)thumb, (int)r);


			//Save a cached version
			if(String.IsNullOrEmpty(context.Request.QueryString["NoCache"]))
				if(!String.IsNullOrEmpty(context.Request["quality"]))
					SaveJpeg(cache + cache_name, thumb, int.Parse(context.Request["quality"]));
				else
					SaveJpeg(cache + cache_name, thumb, 100);			
					//thumb.Save(cache + cache_name, System.Drawing.Imaging.ImageFormat.Jpeg);


			//"Write" the thumbimage
			context.Response.ContentType = "image/jpeg";
			thumb.Save(context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            //context.Response.Write(cache_name);
			thumb.Dispose();
			context.Response.End();
		}
		catch (Exception ex)
		{
			context.Response.Write(ex.Message);
		}

    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

	private Bitmap ResizeImage(Bitmap input, int width, int height)
	{
		Bitmap output = new Bitmap(width, height, input.PixelFormat);
		output.SetResolution(96, 96);
		Graphics g = Graphics.FromImage(output);
		g.Clear(Color.White);
		
		g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

		g.DrawImage(input, new Rectangle(0, 0, output.Width, output.Height), 0, 0, input.Width, input.Height, GraphicsUnit.Pixel);
		g.Dispose();

		return output;
	}

	private Bitmap EnlargeCanvas(Bitmap input, int width, int height)
	{
		//create new image
		Bitmap output = new Bitmap(width, height);

		output.SetResolution(96, 96);
		//create the graphics object to draw with
		Graphics g = Graphics.FromImage(output);
		
		g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

		g.Clear(Color.White);
		Rectangle rect = new Rectangle((width-input.Width)/2, (height-input.Height)/2, width, height);

		//draw the rectCropArea of the original image to the rectDestination of bmpCropped
		g.DrawImage(input, rect, new Rectangle(0,0,width, height), GraphicsUnit.Pixel);

		//release system resources
		g.Dispose();
		input.Dispose();
		return output;
	
	}

	private Bitmap CropImage(Bitmap input, int width, int height)
	{
		//create new image
		Bitmap output = new Bitmap(width, height);
		output.SetResolution(96, 96);
		//create the graphics object to draw with
		Graphics g = Graphics.FromImage(output);
		
		g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

		Rectangle destRect = new Rectangle(0, 0, width, height);
		Rectangle srcRect = new Rectangle((input.Width - width)/2, (input.Height - height)/2, width, height);  
		//draw the rectCropArea of the original image to the rectDestination of bmpCropped
		g.DrawImage(input, destRect, srcRect, GraphicsUnit.Pixel);

		//release system resources
		g.Dispose();
		input.Dispose();
		return output;
	}

	private Bitmap CreateRoundedCorners(Bitmap input, int radius)
	{
		Bitmap output = new Bitmap(input.Width, input.Height);

		
		Graphics g = Graphics.FromImage(output);
		g.Clear(Color.White);	
		Rectangle rect = new Rectangle(0, 0, output.Width, output.Height);

		TextureBrush brush = new TextureBrush(input);

		System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
		gp.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
		gp.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
		gp.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
		gp.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
		gp.AddLine(rect.X, rect.Y + rect.Height - radius, rect.X, rect.Y + radius / 2);

		g.FillPath(brush, gp);
		g.Dispose();

		return output;
	}
	
	public static void SaveJpeg(string path, Image image, int quality)
        {
            //ensure the quality is within the correct range
            if ((quality < 0) || (quality > 100))
            {
                //create the error message
                string error = string.Format("Jpeg image quality must be between 0 and 100, with 100 being the highest quality.  A value of {0} was specified.", quality);
                //throw a helpful exception
                throw new ArgumentOutOfRangeException(error);
            }

            //create an encoder parameter for the image quality
            EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            //get the jpeg codec
            ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");

            //create a collection of all parameters that we will pass to the encoder
            EncoderParameters encoderParams = new EncoderParameters(1);
            //set the quality parameter for the codec
            encoderParams.Param[0] = qualityParam;
            //save the image using the codec and the parameters
            image.Save(path, jpegCodec, encoderParams);
        }

	private static Dictionary<string, ImageCodecInfo> encoders = null;
	public static Dictionary<string, ImageCodecInfo> Encoders
	{
		//get accessor that creates the dictionary on demand
		get
		{
			//if the quick lookup isn't initialised, initialise it
			if (encoders == null)
			{
				encoders = new Dictionary<string, ImageCodecInfo>();
			}

			//if there are no codecs, try loading them
			if (encoders.Count == 0)
			{
				//get all the codecs
				foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
				{
					//add each codec to the quick lookup
					encoders.Add(codec.MimeType.ToLower(), codec);
				}
			}

			//return the lookup
			return encoders;
		}
	}

	
        /// <summary >
        /// Returns the image codec with the given mime type 
        /// </summary> 
        public static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            //do a case insensitive search for the mime type
            string lookupKey = mimeType.ToLower();

            //the codec to return, default to null
            ImageCodecInfo foundCodec = null;

            //if we have the encoder, get it to return
            if (Encoders.ContainsKey(lookupKey))
            {
                //pull the codec from the lookup
                foundCodec = Encoders[lookupKey];
            }

            return foundCodec;
        } 

}