using System;
using System.IO;
using System.Text;

namespace XPloit.Server.Http
{
    public class HttpMultiPartParser
    {
        bool _error = false;
        Encoding codec = null;
        HttpFile[] _files = new HttpFile[] { };
        Var[] _vars = new Var[] { };

        public bool HasError { get { return _error; } }
        public HttpFile[] Files { get { return _files; } }
        public Var[] Vars { get { return _vars; } }

        string name_var = null;
        StringBuilder variable_val = null;
        FileStream cur = null;

        int BOUNDARY_LG = 0, BUF_SIZE = 0, BOUNDARY_LGE = 0;
        string BOUNDARY = "", BOUNDARY_END = "";

        public class Var
        {
            string _n, _v;
            public string Name { get { return _n; } }
            public string Value { get { return _v; } }

            public Var(string name, string value) { _n = name; _v = value; }
            public override string ToString() { return _n + "=" + _v; }
        }

        static int IndexOfLine(byte[] searchin, int start, int max_pos)
        {
            if (max_pos < 2 || start >= max_pos) return -1;

            int ix = start;
            while ((ix = Array.IndexOf<byte>(searchin, 13, ix, max_pos - ix - 1)) != -1)
            {
                if (searchin[ix + 1] == 10)
                    return ix + 2;
                ix++;
            }
            return -1;
        }

        public HttpMultiPartParser(Encoding codec_post, string contenttype, int buf_size)
        {
            BUF_SIZE = buf_size;
            codec = codec_post;
            int boundary_index = contenttype.IndexOf("boundary=") + 9;
            string boundary = "--" + contenttype.Substring(boundary_index, contenttype.Length - boundary_index);

            BOUNDARY = boundary + "\r\n";
            BOUNDARY_END = boundary + "--" + "\r\n";
            BOUNDARY_LG = BOUNDARY.Length;
            BOUNDARY_LGE = BOUNDARY_END.Length;
        }

        public void Process(Stream stream, int content_len, HttpPostProgress prog)
        {
            if (content_len <= 0) return;

            byte[] buf = new byte[BUF_SIZE];
            int to_read = content_len, numread = 0;

            try
            {
                bool in_file = false;
                numread = stream.Read(buf, 0, BOUNDARY_LG);
                to_read -= numread;

                if (codec.GetString(buf, 0, BOUNDARY_LG) != BOUNDARY || to_read == 0) throw (new Exception("DATA ERROR"));

                byte[] last = new byte[] { };
                int parm = 30000, part = 1500;
                if (prog != null) prog.UpdateValue(numread, false);
                StringBuilder header = new StringBuilder();

                int sizen = 0;
                do
                {
                    Array.Resize<byte>(ref last, sizen + BUF_SIZE);

                    numread = stream.Read(last, sizen, BUF_SIZE);
                    if (numread <= 0)
                    {
                        if (to_read == 0) break;
                        else { throw new Exception("Client disconnected during Post"); }
                    }

                    to_read -= numread;
                    sizen += numread;

                    int ix = 0, pos = 0;
                    while ((ix = IndexOfLine(last, pos, sizen)) != -1)
                    {
                        //PROCESAR LINEAS
                        int lg = ix - pos;
                        if (!in_file)
                        {
                            //HEADER
                            if (lg == 2) { MakeHeader(header.ToString()); header.Clear(); in_file = true; }
                            else { header.Append(codec.GetString(last, pos, lg)); }
                        }
                        else
                        {
                            if (lg == BOUNDARY_LG || lg == BOUNDARY_LGE)
                            {
                                string cad = codec.GetString(last, pos, lg);
                                //BOUNDARY ENCONTRADO
                                if (cad == BOUNDARY || cad == BOUNDARY_END) { EndMultiPart(); in_file = false; }
                            }
                            //archivo
                            if (in_file) { PartMultipart(last, pos, lg); }
                        }
                        pos = ix;
                    }
                    if (pos > 0)
                    {
                        //ELIMINAR LO YA PROCESADO
                        sizen -= pos;
                        if (sizen == 0) last = new byte[] { };
                        else
                        {
                            byte[] bx = new byte[sizen];
                            Array.Copy(last, pos, bx, 0, sizen);
                            last = bx;
                        }
                    }

                    // CONTROL DE LINEAS GRANDES
                    if (in_file && sizen > parm)
                    {
                        int iput = sizen - part;
                        PartMultipart(last, 0, iput);

                        byte[] bx = new byte[part];
                        Array.Copy(last, iput, bx, 0, part);
                        last = bx;
                        sizen = part;
                    }

                    if (prog != null) //progreso
                    {
                        prog.UpdateValue(numread, true);
                        if (prog.Cancel) throw new Exception("Client disconnected");
                    }

                } while (to_read > 0);
            }
            catch { _error = true; }
            finally { EndMultiPart(); }
        }

        void EndMultiPart()
        {
            if (cur != null)
            {
                long lg = cur.Length;
                if (lg > 0) { lg -= 2; cur.SetLength(lg); }

                cur.Close(); cur.Dispose(); cur = null;
                _files[_files.Length - 1].UpdateLength(lg);
            }
            if (variable_val != null)
            {
                int l = _vars.Length;
                Array.Resize(ref _vars, l + 1);

                string vl = variable_val.ToString();
                /*if (vl.EndsWith("\r\n")) */
                vl = vl.Remove(vl.Length - 2, 2);

                _vars[l] = new Var(name_var, vl);

                variable_val = null;
                name_var = null;
            }
        }
        void MakeHeader(string header)
        {
            string lheader = header.ToLower();
            int filename_index = lheader.IndexOf("filename=\"");
            int name_starts = lheader.IndexOf("name=\"") + 6;
            if (filename_index != -1)
            {
                int filename_starts = filename_index + 10;
                int content_type_starts = lheader.IndexOf("content-type: ") + 14;

                string filename = header.Substring(filename_starts, header.IndexOf("\"", filename_starts) - filename_starts);
                string content_type = header.Remove(0, content_type_starts);
                string name = header.Substring(name_starts, header.IndexOf("\"", name_starts) - name_starts);

                variable_val = null;
                string tmp = Path.GetTempFileName();
                cur = new FileStream(tmp, FileMode.Create, FileAccess.ReadWrite);

                int ln = _files.Length;
                Array.Resize(ref _files, ln + 1);
                _files[ln] = new HttpFile(name, filename, content_type, tmp);
            }
            else
            {
                //content-transfer-encoding
                variable_val = new StringBuilder();
                name_var = header.Substring(name_starts, header.IndexOf("\"", name_starts) - name_starts);
                if (cur != null) { cur.Close(); cur.Dispose(); cur = null; }
            }
        }
        void PartMultipart(byte[] buf, int ix, int lg)
        {
            if (lg <= 0) return;

            if (cur == null)
            {
                if (variable_val != null) variable_val.Append(codec.GetString(buf, ix, lg));
            }
            else { cur.Write(buf, ix, lg); }
        }
    }
}