namespace Plutonium
{
    public static class ByteUtil
    {
        private static readonly string[] Pack =
        {
            "t=\"2\"/></USER><USER login=\"", ""+'\x01'+"^", "<TURN><USER login=\"", ""+'\x01'+"r", "></USER><USER login=\"",
            ""+'\x01'+"W", "\"/><MAP v=\"", ""+'\x01'+"f", "\" t=\"2\"/><a sf=\"", ""+'\x01'+"M", "\" t=\"1\" direct=\"", ""+'\x01'+"N",
            "><a sf=\"0\" t=\"", ""+'\x01'+"`", "\" t=\"5\" xy=\"", ""+'\x01'+"c", ".1\" slot=\"", ""+'\x01'+"j", "\" quality=\"",
            ""+'\x01'+"l", "\" massa=\"1", ""+'\x01'+"m", "\" maxquality=\"", ""+'\x01'+"n", "><a sf=\"6\" t=\"2\"/><a ", ""+'\x01'+"B",
            "/><a sf=\"6\" t=\"", ""+'\x01'+"o", "\" damage=\"S", ""+'\x01'+"p", "\" made=\"AR$\" ", ""+'\x01'+"q", "\" nskill=\"",
            ""+'\x01'+"s", "\" st=\"G,H\" ", ""+'\x01'+"t", "\" type=\"1\"", ""+'\x01'+"u", "section=\"0\" damage=\"", ""+'\x01'+"~",
            "\" section=\"", ""+'\x01'+"", "=\"1\" type=\"", ""+'\x01'+"A", "protect=\"S", ""+'\x01'+"C", " ODratio=\"1\" loc_time=\"",
            ""+'\x01'+"V", "\"/>\n</O>\n<O id=\"", ""+'\x01'+"D", "\"/>\n<O id=\"", ""+'\x01'+"E", "level=", ""+'\x01'+"F", " min=\"", ""+'\x01'+"H",
            " txt=\"ammo ", ""+'\x01'+"I", " txt=\"BankCell Key (copy) #", ""+'\x01'+"J", "\" txt=\"Coins\" massa=\"1\" ", ""+'\x01'+"K",
            " cost=\"0\" ", ""+'\x01'+"L",
            ".1\" name=\"b1-g2\" txt=\"Boulder\" massa=\"5\" st=\"G,H\" made=\"AR$\" section=\"0\" damage=\"S2-5\" shot=\"7-1\" nskill=\"4\" OD=\"1\" type=\"9.1\"/>",
            ""+'\x01'+"S", " psy=\"0\" man=\"1\" maxHP=\"", ""+'\x01'+"T", " freeexchange=\"1\" ", ""+'\x01'+"U",
            "\" virus=\"0\" login=\"", ""+'\x01'+"Y", "\" ne=\",,,,,\" ne2=\",,,,,\" nark=\"0\" gluk=\"0\" ", ""+'\x01'+"Z",
            "\" max_count=\"", ""+'\x01'+"[", "\" calibre=\"", ""+'\x01'+"\\", "\" count=\"", ""+'\x01'+"]", "\" build_in=\"", ""+'\x01'+"O",
            "\" shot=\"", ""+'\x01'+"_", "\" range=\"", ""+'\x01'+"a", ".1\" slot=\"A\" name=\"b", ""+'\x01'+"b",
            ".1\" slot=\"B\" name=\"b", ""+'\x01'+"d", ".1\" slot=\"C\" name=\"b", ""+'\x01'+"h", ".1\" slot=\"D\" name=\"b",
            ""+'\x01'+"e", ".1\" slot=\"E\" name=\"b", ""+'\x01'+"g", ".1\" slot=\"F\" name=\"b", ""+'\x01'+"{",
            ".1\" slot=\"GH\" name=\"b", ""+'\x01'+"v", "\" slot=\"", ""+'\x01'+"X",
            " psy=\"0\" man=\"3\" maxPsy=\"0\" ODratio=\"1\" img=\"rat\" group=\"2\" battleid=\"", ""+'\x01'+"i",
            ".1\" name=\"b2-s5\" txt=\"Silicon\" massa=\"50\" ", ""+'\x01'+"k",
            ".1\" name=\"b2-s8\" txt=\"Venom\" massa=\"70\" ", ""+'\x01'+"Q",
            ".1\" name=\"b2-s4\" txt=\"Organic\" massa=\"30\" ", ""+'\x01'+"w",
            ".1\" name=\"b2-s2\" txt=\"Precious metals\" massa=\"500\" ", ""+'\x01'+"x",
            ".1\" name=\"b2-s7\" txt=\"Gems\" massa=\"80\" ", ""+'\x01'+"y",
            ".1\" name=\"b2-s6\" txt=\"Radioactive materials\" massa=\"800\" ", ""+'\x01'+"z",
            ".1\" name=\"b2-s3\" txt=\"Polymers\" massa=\"30\" ", ""+'\x01'+"|",
            "<BATTLE t=\"45\" t2=\"45\" turn=\"1\" cl=\"0\" ", ""+'\x01'+"}", "\" ODratio=\"1\" ", ""+'\x01'+"P",
            "\" p=\"\"/></L><L X=\"", ""+'\x01'+"R", "zzzzzz", ""+'\x01'+"G", "\"/><a sf=\"", "\x02", ">\n<O id=\"", "\x03",
            "><O id=\"", "\x04", "      ", "\x05", "00\" ", "\x06", " txt=\"", "\x07", " name=\"b", "\b"
        };




        public static string Unpack(string input)
        {
            for (int i = Pack.Length - 1; i >= 0; i = i - 2)
            {
                input = input.Replace(Pack[i], Pack[i - 1]);
            }

            return input;
        }
        public static byte[] ChompBytes(byte[] bzBytes, int offset, int numBytes)
        {
            if (numBytes > bzBytes.Length)
                numBytes = bzBytes.Length;

            if (numBytes < 0)
                numBytes = 0;

            var bzChunk = new byte[numBytes];
            for (var x = 0; x < numBytes; x++)
            {
                bzChunk[x] = bzBytes[offset];
                offset++;
            }

            return bzChunk;
        }
    }
}