using System;

using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace OpenKh.Command.AnbLister
{
	class Program
	{
		const string Auteur = "Soraiko";
		const string Version = "1.1";
		public static byte[] SubArray(byte[] data, long index, long length) {byte[] result = new byte[length];Array.Copy(data, index, result, 0, length);return result;}
		public static byte[] Combine(byte[] a, byte[] b) {byte[] c = new byte[a.Length + b.Length];  System.Buffer.BlockCopy(a, 0, c, 0, a.Length); System.Buffer.BlockCopy(b, 0, c, a.Length, b.Length);  return c;}
		
		public static void Main(string[] args)
		{
			Console.Title = "KH2FM_PS2_ANB_MassMsetMaker_["+Auteur+"]_"+Version;
			
			if (args.Length>1)
			{
				if (Path.GetFileName(args[0])=="obj")
				{
					obj_folder = args[0];
				}
				else if (Path.GetFileName(args[0])=="anm")
				{
					anm_folder = args[0];
				}
				if (Path.GetFileName(args[1])=="anm")
				{
					anm_folder = args[1];
				}
				else if (Path.GetFileName(args[1])=="obj")
				{
					obj_folder = args[1];
				}
				if (anm_folder!=""&obj_folder!="")
				{
					Console.Write("Press enter to start with:\r\n"+anm_folder+@"\**\*.anb"+"\r\n"+obj_folder+"*.mdlx\r\n\r\n");
					Console.ReadKey(true);
					Directory.CreateDirectory(anm_folder.Substring(0,anm_folder.Length)+"_converted");
					Process.Start(anm_folder.Substring(0,anm_folder.Length)+"_converted");
					new_anm_folder = anm_folder.Substring(0,anm_folder.Length)+"_converted";
					string[] directories = Directory.GetDirectories(anm_folder+@"\");
					GetFiles_(anm_folder+@"\");
					foreach (string directory in directories)
					{
						FolderToLower(directory);
					}
				}
				else
				{
					Console.Write("Please drop the \"anm\" AND \"obj\" folder on this app.\r\n\r\nPress any key to continue . . .");
					Console.ReadKey(true);
				}
			}
			else
			{
				Console.Write("Please drop the \"anm\" AND \"obj\" folder on this app.\r\n\r\nPress any key to continue . . .");
				Console.ReadKey(true);
			}
		}
		
		public static void GetFiles_(string dir)
		{
			try
			{
				get_perc+=(long)Directory.GetFiles(dir).Length;
			}
			catch
			{
				
			}
				try
				{
					foreach (string directoryz in Directory.GetDirectories(dir+@"\"))
					{
						GetFiles_(directoryz);
					}
				}
				catch
				{
					
				}
		}
		
		//Lire entier 32bits
		public static int ReadInteger4(int address)
		{
			try 
			{
				return BitConverter.ToInt32(new byte [] {curr_mset[address],curr_mset[address+1],curr_mset[address+2],curr_mset[address+3]},0);
			}
			catch 
			{
				return 0;
			}
		}
		
		//Lire entier 16bits
		public static int ReadInteger2(int address)
		{
			try 
			{
				return (int)BitConverter.ToInt16(curr_mset, address);
			}
			catch 
			{
				return 0;
			}
		}
		
		//Lire entier 8bits
		public static int ReadInteger1(int address)
		{
            try
            {
                return (int)curr_mset[address];
            }
			catch
			{
				return 0;
			}
		}
		
		//Ecrire entier 32bits
		static void WriteInteger(int address,int valeur)
		{
			try 
			{
				byte[] out_ = BitConverter.GetBytes(valeur);
				curr_mset[address] = out_[0];
				curr_mset[address+1] = out_[1];
				curr_mset[address+2] = out_[2];
				curr_mset[address+3] = out_[3];
			}
			catch
			{
				
			}
		}
		
		public static string obj_folder ="";
		public static string anm_folder ="";
		public static string new_anm_folder="";
		public static string curr_obj="";
		
		public static byte[] curr_mset = new byte[0];
		public static string curr_mdlx = " ";
		public static List<string> ANBs_to_concat = new List<string>(0);
		public static byte[] curr_anb = new byte[0];
		public static long get_perc = 0;
		public static long get_percount = 0;
		
		
		static void FolderToLower(string path)
		{
			string name_ = Path.GetFileName(path.Substring(0,Path.GetDirectoryName(path).Length));
			if (Directory.Exists(new_anm_folder+@"\"+name_)==false)
			{
				Directory.CreateDirectory(new_anm_folder+@"\"+name_);
			}
			string[] files = Directory.GetFiles(path+@"\");
			if (files.Length>0)
			{
				foreach (string file in files)
				{
					Console.Title = "KH2FM_PS2_ANB_MassMsetMaker_["+Auteur+"]_"+Version+"  "+ (((double)get_percount/(double)get_perc)*100).ToString()+" %";
					get_percount++;
					if (curr_obj != Path.GetFileName(Path.GetDirectoryName(file)))
					{
						if (curr_mdlx.Length>1)
						{
							string text = "";
							int count = 0;
							curr_mset= new byte[] {0x42,0x41,0x52,0x01,0,0,0,0,0,0,0,0,1,0,0,0};
							WriteInteger(4,(int)ANBs_to_concat.Count);
							
							foreach (string anb_ in ANBs_to_concat.ToArray())
							{
								byte[] curr_anb = File.ReadAllBytes(anb_);
								try
								{
									int address = curr_anb.Length-4;
									
									int curr = BitConverter.ToInt32(new byte [] {curr_anb[address],curr_anb[address+1],curr_anb[address+2],curr_anb[address+3]},0);
									int previous = BitConverter.ToInt32(new byte [] {curr_anb[address-4],curr_anb[address+1-4],curr_anb[address+2-4],curr_anb[address+3-4]},0);
									while (previous!=1065353216&curr!=0)
									{
										address = address-4;
										previous = BitConverter.ToInt32(new byte [] {curr_anb[address-4],curr_anb[address+1-4],curr_anb[address+2-4],curr_anb[address+3-4]},0);
										curr = BitConverter.ToInt32(new byte [] {curr_anb[address],curr_anb[address+1],curr_anb[address+2],curr_anb[address+3]},0);
										
									}
									curr_anb[address] = 0;
									curr_anb[address+1] = 0;
									curr_anb[address+2] = 0;
									curr_anb[address+3] = 0;
										
								}
								catch
								{
									
								}
								curr_mset = Combine(curr_mset,new byte[] {0x11,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0});
								count ++;
							}
							count =0;
							foreach (string anb_ in ANBs_to_concat.ToArray())
							{
								text+= "M"+count.ToString("d3")+" = anb"+anb_.Substring(anm_folder.Length,anb_.Length-anm_folder.Length).Replace(@"\","/")+"\r\n";
								curr_anb = new byte[0];
								curr_anb = File.ReadAllBytes(anb_);
								curr_mset = Combine(curr_mset,curr_anb);
								byte[] name = System.Text.Encoding.ASCII.GetBytes("M"+count.ToString("d3"));
								curr_mset[16+(16*count)+4] = name[0];
								curr_mset[16+(16*count)+5] = name[1];
								curr_mset[16+(16*count)+6] = name[2];
								curr_mset[16+(16*count)+7] = name[3];
								WriteInteger(16+(16*count)+8,curr_mset.Length-curr_anb.Length);
								WriteInteger(16+(16*count)+12,curr_anb.Length);
								count ++;
							}
							
							if (File.Exists(curr_mdlx+".mset")==false)
							{
								File.WriteAllText(curr_mdlx+".txt",text);
								File.WriteAllBytes(curr_mdlx+".mset",curr_mset);
							}
							
							curr_mset = new byte[0];
							curr_mdlx = "";
						}
						if (File.Exists(obj_folder+@"\"+Path.GetFileName(Path.GetDirectoryName(file))+".mdlx")&File.Exists(new_anm_folder+@"\"+name_+@"\"+Path.GetFileName(Path.GetDirectoryName(file))+".mdlx")==false)
						{
							curr_mdlx = new_anm_folder+@"\"+name_+@"\"+Path.GetFileName(Path.GetDirectoryName(file));
							Console.Write("Creating "+new_anm_folder+@"\"+Path.GetFileName(Path.GetDirectoryName(file))+".mdlx...\r\n");
							File.Copy(obj_folder+@"\"+Path.GetFileName(Path.GetDirectoryName(file))+".mdlx",new_anm_folder+@"\"+name_+@"\"+Path.GetFileName(Path.GetDirectoryName(file))+".mdlx");
						}
						else if (File.Exists(new_anm_folder+@"\"+name_+@"\"+Path.GetFileName(Path.GetDirectoryName(file))+".mdlx")==false)
						{
							string [] objs = Directory.GetFiles(obj_folder+@"\");
							for (int i=0;i<objs.Length;i++)
							{
								string curr=Path.GetFileNameWithoutExtension(objs[i]).ToLower();
								if (curr.Contains(Path.GetFileName(Path.GetDirectoryName(file)).ToLower()))
								{
									curr_mdlx = new_anm_folder+@"\"+name_+@"\"+Path.GetFileName(Path.GetDirectoryName(file));
									Console.Write("Creating "+new_anm_folder+@"\"+name_+@"\"+Path.GetFileName(Path.GetDirectoryName(file))+".mdlx...\r\n");
									File.Copy(objs[i],new_anm_folder+@"\"+name_+@"\"+Path.GetFileName(Path.GetDirectoryName(file))+".mdlx");
									try
									{
										byte[] mdlx = File.ReadAllBytes(new_anm_folder+@"\"+name_+@"\"+Path.GetFileName(Path.GetDirectoryName(file))+".mdlx");
										
										int count = 0;
										int max = BitConverter.ToInt32(new byte [] {mdlx[4],mdlx[5],mdlx[+6],mdlx[7]},0);
										while (mdlx[count*16]!=0x17&count<max+1)
										{
											count++;
										}
										int consistance = 0;
										
										if (mdlx[count*16]!=0x17)
										{
											mdlx = Combine(Combine(SubArray(mdlx,0,count*16),new byte[] {23,0,0,0,mdlx[(count*16)-12],mdlx[(count*16)-11],mdlx[(count*16)-10],mdlx[(count*16)-9],0x60,0,0,0,0,0,0,0}),SubArray(mdlx,(count*16),mdlx.Length-(count*16)));
											byte[] length = BitConverter.GetBytes(mdlx.Length);
											
											mdlx[count*16+8]=length[0];
											mdlx[count*16+9]=length[1];
											mdlx[count*16+10]=length[2];
											mdlx[count*16+11]=length[3];
											mdlx[mdlx.Length]=0x13;
											mdlx[mdlx.Length+4]=1;
											mdlx[mdlx.Length+(5*16)]=0x5a;
											mdlx[mdlx.Length+(5*16)+2]=0x5a;
										}
										else if (mdlx[count*16]==0x17)
										{
											consistance = BitConverter.ToInt32(new byte [] {mdlx[8+(count*16)],mdlx[9+(count*16)],mdlx[10+(count*16)],mdlx[11+(count*16)]},0);
											mdlx=Combine(mdlx,new byte[0x60]);
											mdlx[consistance] = 0x24;
											mdlx[consistance+4] = 1;
											mdlx[consistance+0x50] = 0x5A;
											mdlx[consistance+0x52] = 0x5A;
										}
										File.WriteAllBytes(new_anm_folder+@"\"+name_+@"\"+Path.GetFileName(Path.GetDirectoryName(file))+".mdlx",mdlx);
									}
									catch
									{
										
									}
									objs=new string[0];
								}
							}
						}
						ANBs_to_concat.Clear();
					}
					else
					{
						ANBs_to_concat.Add(file);
					}
					curr_obj = Path.GetFileName(Path.GetDirectoryName(file));
					Console.Write(Path.GetFileName(file)+"\r\n");
					Console.Write(file+"\r\n");
				}
			}
			else
			{
				string[] directories = Directory.GetDirectories(path+@"\");
				foreach (string directory in directories)
				{
					FolderToLower(directory);
				}
			}
		}
	}
}