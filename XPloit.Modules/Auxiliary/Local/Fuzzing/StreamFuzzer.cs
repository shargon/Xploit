﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Interfaces;
using XPloit.Core.Requirements.Payloads;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;

namespace Auxiliary.Local.Fuzzing
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Generic Fuzzer")]
    public class StreamFuzzer : Module
    {
        /// <summary>
        /// Interface for payload
        /// </summary>
        public interface IFuzzerPayload
        {
            Stream CreateStream(byte[] data);
        }

        /// <summary>
        /// Class for script
        /// </summary>
        public class ScriptClass : IDisposable
        {
            Encoding _Encoding = Encoding.ASCII;
            Stream _Stream;

            public Stream Stream { get { return _Stream; } set { _Stream = value; } }
            public Encoding Encoding { get { return _Encoding; } set { _Encoding = value; } }

            public virtual void Run(byte[] fuzzData) { }

            public int Read(byte[] data, int index, int length) { return _Stream.Read(data, index, length); }
            public string ReadLine() { return ReadLine(_Encoding); }
            public string ReadLine(Encoding encoding)
            {
                List<byte> ls = new List<byte>();

                while (true)
                {
                    int ix = _Stream.ReadByte();
                    if (ix == -1 | ix == '\n') break;
                    ls.Add((byte)ix);
                }

                string ret = encoding.GetString(ls.ToArray());
                return ret;
            }
            public byte Read()
            {
                int ix = _Stream.ReadByte();
                if (ix == -1) throw (new Exception("EOF"));
                return (byte)ix;
            }

            public void Write(byte[] data, bool flush = true) { Write(data, 0, data.Length, flush); }
            public void Write(string input) { Write(_Encoding, input); }
            public void Write(Encoding codec, string input) { Write(codec.GetBytes(input)); }
            public void Write(byte[] data, int index, int length, bool flush = true)
            {
                _Stream.Write(data, index, length);
                if (flush) _Stream.Flush();
            }

            public void Dispose()
            {
                if (_Stream != null)
                {
                    _Stream.Dispose();
                    _Stream = null;
                }
            }
        }

        #region Configure
        public override IPayloadRequirements PayloadRequirements { get { return new InterfacePayload(typeof(IFuzzerPayload)); } }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "From Length")]
        public int From { get; set; }
        [ConfigurableProperty(Description = "To Length")]
        public int To { get; set; }
        [ConfigurableProperty(Description = "Step")]
        public int Step { get; set; }
        [ConfigurableProperty(Description = "Create pattern")]
        public bool CreatePattern { get; set; }
        [ConfigurableProperty(Description = "Char for send when CreatePattern is false")]
        public char NotPatternChar { get; set; }
        [RequireExists()]
        /*
            // VULNSERVER
            public override void Run(byte[] fuzzData)
            {
              ReadLine();
              Write(Encoding.GetBytes("TRUN .").Concat(fuzzData));
              ReadLine();
              Write("EXIT");
              ReadLine();
            }
        */
        [ConfigurableProperty(Description = "Script for replay")]
        public FileInfo Script { get; set; }
        [ConfigurableProperty(Description = "Encoding ussed")]
        public Encoding Encoding { get; set; }
        #endregion

        public StreamFuzzer()
        {
            From = 1;
            To = 5000;
            Step = 1;
            CreatePattern = true;
            NotPatternChar = 'A';
            Encoding = Encoding.ASCII;
        }

        public override bool Run()
        {
            IFuzzerPayload payload = (IFuzzerPayload)Payload;

            // Make script with inherited class
            ScriptHelper scripts = ScriptHelper.CreateFromFile(Script.FullName, new ScriptHelper.ScriptOptions()
            {
                includeUsings = new string[]
                {
                    "XPloit.Core.Extensions",
                    "XPloit.Core.Helpers"
                },
                IncludeFiles = new string[]
                {
                    typeof(ScriptClass).Assembly.Location,    // XPloit.Modules
                    typeof(Module).Assembly.Location          // XPloit.Core
                },
                Inherited = new Type[]
                {
                    typeof(StreamFuzzer.ScriptClass)
                }
            });

            WriteInfo("Loading file ...");
            string file = File.ReadAllText(Script.FullName, Encoding.ASCII);
            WriteInfo("File loaded", StringHelper.Convert2KbWithBytes(file.Length), System.ConsoleColor.Green);

            for (int max = To, st = Math.Max(Step, 1); From <= max; From += st)
            {
                WriteInfo("Checking ", From.ToString(), ConsoleColor.Green);

                byte[] data;

                if (CreatePattern)
                    data = PatternHelper.CreateRaw(From);
                else
                {
                    data = new byte[From];
                    for (int x = data.Length - 1; x >= 0; x--) data[x] = (byte)NotPatternChar;
                }

                ScriptClass obj = scripts.CreateNewInstance<ScriptClass>();
                obj.Encoding = Encoding;

                try
                {
                    obj.Stream = payload.CreateStream(data);
                    obj.Run(data);
                    obj.Dispose();

                    CopyPropertiesToActiveModule("From");
                }
                catch (Exception e)
                {
                    WriteInfo("Good news :) Payload send fail!");
                    WriteError(e.Message);
                    return true;
                }
            }

            return true;
        }
    }
}