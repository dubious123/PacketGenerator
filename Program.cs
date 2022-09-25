using System.Collections.Generic;
using System.IO;

namespace PacketGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			var packetsPath = "../../../Packets.cs";
			var clientPacketLines = new LinkedList<string>();
			var serverPacketLines = new LinkedList<string>();
			var lines = File.ReadAllLines(packetsPath);
			var packetNames = new List<string>();
			int clientCount = 0;
			int serverCount = 0;
			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i];
				if (((line.Contains("public class") || line.Contains("public struct")) && line.Contains("_")) == false) continue;
				var startIndex = line.IndexOf("_") - 1;
				var endIndex = line.IndexOf(" :");
				var name = line.Substring(startIndex, endIndex - startIndex);
				packetNames.Add(name);

				i += 4;
				lines[i] = name.Contains("C_") ? $"\t\t\tId = 0x0{clientCount++:X3};" : $"\t\t\tId = 0x1{serverCount++:X3};";
			}

			int index = 0;
			//C_가 포함된 line까지 두 linkedlist에 넣는다.
			for (index = 0; lines[index].Contains("C_") == false; index++)
			{
				var line = lines[index];
				if (line.Contains("#region Server"))
				{
					for (index++; lines[index] != "#endregion"; index++)
					{
						serverPacketLines.AddLast(lines[index]);
					}
					continue;
				}
				if (line.Contains("#region Client"))
				{
					for (index++; lines[index] != "#endregion"; index++)
					{
						clientPacketLines.AddLast(lines[index]);
					}
					continue;
				}
				if (lines[index].Contains("namespace"))
				{
					serverPacketLines.AddLast(line);
					serverPacketLines.AddLast(lines[++index]);
					continue;
				}
				clientPacketLines.AddLast(line);
				serverPacketLines.AddLast(line);

			}

			//C_가 포함되어있으면 client에는 생성자가 포함되게, server에는 생성자가 포함 안되게 넣는다.
			for (int i = index; i < lines.Length;)
			{
				var line = lines[i];
				if (line.Contains("public class C_") || line.Contains("public struct C_"))
				{
					//2줄 넣기
					clientPacketLines.AddLast(lines[i]);
					serverPacketLines.AddLast(lines[i++]);
					clientPacketLines.AddLast(lines[i]);
					serverPacketLines.AddLast(lines[i++]);
					for (; line != "\t}";)
					{
						line = lines[i];
						//생성자는 client만 넣기
						if (line.Contains("\t\tpublic C_"))
						{
							for (; line != "\t\t}"; line = lines[++i])
							{
								clientPacketLines.AddLast(line);
							}
							clientPacketLines.AddLast(line);
							i++;
						}
						//struct 
						else if (line.Contains("public struct"))
						{
							//2줄 넣기
							clientPacketLines.AddLast(lines[i]);
							serverPacketLines.AddLast(lines[i++]);
							clientPacketLines.AddLast(lines[i]);
							serverPacketLines.AddLast(lines[i++]);
							//struct 생성자
							line = lines[i];
							for (; line != "\t\t\t}"; line = lines[++i])
							{
								clientPacketLines.AddLast(line);
							}
							clientPacketLines.AddLast(line);
							i++;

							//struct 나머지
							for (; line != "\t\t}";)
							{
								line = lines[i++];
								clientPacketLines.AddLast(line);
								serverPacketLines.AddLast(line);
							}
						}
						else
						{
							clientPacketLines.AddLast(lines[i]);
							serverPacketLines.AddLast(lines[i++]);
						}
					}
				}
				else if (line.Contains("public class S_") || line.Contains("public struct S_"))
				{
					//2줄 넣기
					clientPacketLines.AddLast(lines[i]);
					serverPacketLines.AddLast(lines[i++]);
					clientPacketLines.AddLast(lines[i]);
					serverPacketLines.AddLast(lines[i++]);
					for (; line != "\t}";)
					{
						//생성자는 client만 넣기
						line = lines[i];
						if (line.Contains("\t\tpublic S_"))
						{
							for (; line != "\t\t}"; line = lines[++i])
							{
								serverPacketLines.AddLast(line);
							}
							serverPacketLines.AddLast(line);
							i++;
						}
						//struct 
						else if (line.Contains("public struct"))
						{
							//2줄 넣기
							clientPacketLines.AddLast(lines[i]);
							serverPacketLines.AddLast(lines[i++]);
							clientPacketLines.AddLast(lines[i]);
							serverPacketLines.AddLast(lines[i++]);
							//struct 생성자
							line = lines[i];
							for (; line != "\t\t\t}"; line = lines[++i])
							{
								serverPacketLines.AddLast(line);
							}
							serverPacketLines.AddLast(line);
							i++;

							//struct 나머지
							for (; line != "\t\t}";)
							{
								line = lines[i++];
								clientPacketLines.AddLast(line);
								serverPacketLines.AddLast(line);
							}
						}
						else
						{
							clientPacketLines.AddLast(lines[i]);
							serverPacketLines.AddLast(lines[i++]);
						}
					}
				}
				else
				{
					if (line == "}")
					{
						clientPacketLines.AddLast(line);
						serverPacketLines.AddLast(line);
					}
					i++;
				}
			}



			File.WriteAllLines(packetsPath, lines);
			#region Packets
			var serverPacketsPath = "../../../../Mockup_BrawlStars_Server/Server/Packets.cs";
			var clientPacketsPath = "../../../../Mockup_BrawlStars/Assets/02.Scripts/Network//Packets.cs";
			//var serverPacketsLines = new List<string>(File.ReadAllLines(serverPacketsPath));
			//var clientPacketssLines = new List<string>(File.ReadAllLines(clientPacketsPath));

			File.WriteAllLines(serverPacketsPath, serverPacketLines);
			for (LinkedListNode<string> node = clientPacketLines.First; node != null; node = node.Next)
			{
				if (node.Value.StartsWith("\t"))
				{
					node.Value = node.Value[1..];
					continue;
				}
				if (node.Value == "}")
				{
					clientPacketLines.Remove(node);
					continue;
				}
			}


			File.WriteAllLines(clientPacketsPath, clientPacketLines);
			#endregion

			#region Enums
			var serverEnumPath = "../../../../Mockup_BrawlStars_Server/ServerCore/Utils/Enums.cs";
			var clientEnumPath = "../../../../Mockup_BrawlStars/Assets/02.Scripts/Network/ServerCore/Utils/Enums.cs";
			var serverEnumLines = new List<string>(File.ReadAllLines(serverEnumPath));
			var clientEnumLines = new List<string>(File.ReadAllLines(clientEnumPath));


			EditEnum(packetNames, serverEnumLines);
			EditEnum(packetNames, clientEnumLines);
			File.WriteAllLines(serverEnumPath, serverEnumLines);
			File.WriteAllLines(clientEnumPath, clientEnumLines);
			#endregion
			#region Parser
			var serverParserPath = "../../../../Mockup_BrawlStars_Server/Server/PacketParser.cs";
			var clientParserPath = "../../../../Mockup_BrawlStars/Assets/02.Scripts/Network/PacketParser.cs";
			var serverParserLines = new List<string>(File.ReadAllLines(serverParserPath));
			var clientParserLines = new List<string>(File.ReadAllLines(clientParserPath));
			EditParser(packetNames, serverParserLines, "\t\t", 0, "_readDict.TryAdd((ushort)PacketId.{0}, arr => JsonSerializer.Deserialize<{0}>(arr, _options));");
			EditParser(packetNames, clientParserLines, "\t", 0, "_readDict.TryAdd((ushort)PacketId.{0}, json => JsonUtility.FromJson<{0}>(json));");
			File.WriteAllLines(serverParserPath, serverParserLines);
			File.WriteAllLines(clientParserPath, clientParserLines);
			#endregion

			#region PacketHandler
			var serverPacketHandlerPath = "../../../../Mockup_BrawlStars_Server/Server/PacketHandler.cs";
			var clientPacketHandlerPath = "../../../../Mockup_BrawlStars/Assets/02.Scripts/Network/PacketHandler.cs";
			var serverPacketHandlerLines = new List<string>(File.ReadAllLines(serverPacketHandlerPath));
			var clientPacketHandlerLines = new List<string>(File.ReadAllLines(clientPacketHandlerPath));

			#region Edit Serverside Handler
			{
				var startIndex = serverPacketHandlerLines.FindIndex(line => line.Contains("static PacketHandler()")) + 3;
				var endIndex = serverPacketHandlerLines.FindIndex(line => line.Contains("\t\t}"));
				var format = "_handlerDict.TryAdd(PacketId.{0}, (packet, session) => {0}Handle(packet, session));";
				serverPacketHandlerLines.RemoveRange(startIndex, endIndex - startIndex);
				foreach (var name in packetNames)
				{
					if (name.Contains("C_") == false) continue;
					serverPacketHandlerLines.Insert(startIndex++, "\t\t\t" + string.Format(format, name));

					var methodStart = serverPacketHandlerLines.FindIndex(line => line.Contains($"static void {name}Handle"));
					if (methodStart != -1) continue;
					methodStart = serverPacketHandlerLines.FindLastIndex(line => line.Contains("\t}"));
					serverPacketHandlerLines.Insert(methodStart++, "");
					serverPacketHandlerLines.Insert(methodStart++, $"\t\tprivate static void {name}Handle(BasePacket packet, ClientSession session)");
					serverPacketHandlerLines.Insert(methodStart++, "\t\t{");
					serverPacketHandlerLines.Insert(methodStart++, $"\t\t\tvar req = packet as {name};");
					serverPacketHandlerLines.Insert(methodStart++, "\t\t}");

				}


				File.WriteAllLines(serverPacketHandlerPath, serverPacketHandlerLines);
			}
			#endregion

			#region Edit Clientside Handler
			{
				var startIndex = clientPacketHandlerLines.FindIndex(line => line.Contains("static PacketHandler()")) + 3;
				var endIndex = clientPacketHandlerLines.FindIndex(startIndex, line => line.Contains("\t}"));
				var format = "_handlerDict.TryAdd(PacketId.{0}, (packet,session) => {0}Handle(packet, session));";
				clientPacketHandlerLines.RemoveRange(startIndex, endIndex - startIndex);
				foreach (var name in packetNames)
				{
					if (name.Contains("S_") == false) continue;
					clientPacketHandlerLines.Insert(startIndex++, "\t\t" + string.Format(format, name));

					var methodStart = clientPacketHandlerLines.FindIndex(line => line.Contains($"static void {name}Handle"));
					if (methodStart != -1) continue;
					methodStart = clientPacketHandlerLines.FindLastIndex(line => line.Contains("}"));
					clientPacketHandlerLines.Insert(methodStart++, "");
					clientPacketHandlerLines.Insert(methodStart++, $"\tprivate static void {name}Handle(BasePacket packet, ServerSession session)");
					clientPacketHandlerLines.Insert(methodStart++, "\t{");
					clientPacketHandlerLines.Insert(methodStart++, $"\t\tvar req = packet as {name};");
					clientPacketHandlerLines.Insert(methodStart++, "\t}");
				}


				File.WriteAllLines(clientPacketHandlerPath, clientPacketHandlerLines);

			}
			#endregion

			#endregion

		}

		public static void EditEnum(List<string> names, List<string> lines)
		{
			var startIndex = lines.FindIndex(line => line.Contains("PacketId")) + 2;
			var endIndex = lines.FindIndex(line => line.Contains("}"));
			lines.RemoveRange(startIndex, endIndex - startIndex);
			int clientCount = 0;
			int serverCount = 0;
			foreach (var name in names)
			{
				var line = name.Contains("C_") ? $"\t\t\t{name} = 0x0{clientCount++:X3}," : $"\t\t\t{name} = 0x1{serverCount++:X3},";
				lines.Insert(startIndex++, line);
			}
		}
		public static void EditParser(List<string> names, List<string> lines, string indent, int offset, string format)
		{
			var startIndex = lines.FindIndex(line => line.Contains("_readDict = new ConcurrentDictionary<ushort, ")) + 1 + offset;
			var endIndex = lines.FindIndex(startIndex, line => line.Contains(indent + "}"));
			lines.RemoveRange(startIndex, endIndex - startIndex);
			foreach (var name in names)
			{
				lines.Insert(startIndex++, indent + "\t" + string.Format(format, name));
			}
		}
	}
}
