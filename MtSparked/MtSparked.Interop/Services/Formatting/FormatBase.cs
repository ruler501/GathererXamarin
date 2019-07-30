﻿using System.Collections.Generic;
using MtSparked.Interop.Utils;

namespace MtSparked.Interop.Services.Formatting {

    public class ReverseFormat<Format1, T, FormatResult> where Format1 : class, IFormat<T, FormatResult> {

        public Format1 FormatInstance { get; }

        private ReverseFormat(string name, string description, IDictionary<string, object> formatOptions, string version, bool _) {
            this.Name = name;
            this.Description = description;
            this.FormatOptions = formatOptions;
            this.Version = version;
            this.FormatInstance = this as Format1;
        }

        protected ReverseFormat(string name, string description, IDictionary<string, object> formatOptions, string version)
                : this(name, description, formatOptions, version, true) {
            this.FormatInstance = this as Format1;
        }

        public ReverseFormat(string name, string description, IDictionary<string, object> formatOptions, string version, Format1 format)
                : this(name, description, formatOptions, version, true) {
            this.FormatInstance = format;
        }

        public ReverseFormat(Format1 format)
                : this("Reverse of " + typeof(Format1).Name, "Reverse the Format direction.",
                       new Dictionary<string, object>(), "v1.0", format) { }

        public string Name { get; }
        public string Description { get; }
        public IDictionary<string, object> FormatOptions { get; }
        public string Version { get; }

        public T Format(FormatResult model) => this.FormatInstance.Parse(model);
        public FormatResult Parse(T formattedModel) => this.FormatInstance.Format(formattedModel);

    }

    public abstract class FormatBase<T, FormatResult> : ReverseFormat<FormatBase<T, FormatResult>, T, FormatResult>, IFormat<T, FormatResult> {

        public FormatBase(string name, string description, IDictionary<string, object> formatOptions, string version)
                : base(name, description, formatOptions, version) { }

        public abstract FormatResult Format(T model);
        public abstract T Parse(FormatResult formattedModel);
    }

    public abstract class FormatBase<T, FormatResult, FormattedResult>
            : FormatBase<T, FormattedResult>, IFormat<T, FormatResult, FormattedResult>
            where FormattedResult : Formatted<FormatResult> {
        public FormatBase(string name, string description, IDictionary<string, object> formatOptions, string version)
                : base(name, description, formatOptions, version) { }
    }

    public sealed class AnyFormat<T> : FormatBase<Formatted<T>.Any, T> {

        public AnyFormat()
                : base("Any<" + typeof(T).Name + "> to " + typeof(T).Name, "Add or remove Any qualification",
                       new Dictionary<string, object>(), "v1.0") { }

        // Implicit conversions
        public override T Format(Formatted<T>.Any model) => model;
        public override Formatted<T>.Any Parse(T formattedModel) => formattedModel;

    }

    public abstract class ChainFormatPre<Format1, T, Mid, FormattedMid, Format2, FormatResult, FormattedResult>
              : FormatBase<T, Mid, FormattedMid>,
                IFormat<FormattedMid, FormatResult, FormattedResult>
            where FormattedMid : Formatted<Mid>
            where FormattedResult : Formatted<FormatResult>
            where Format1 : IFormat<T, Mid, FormattedMid>
            where Format2 : IFormat<FormattedMid, FormatResult, FormattedResult> {

        internal ChainFormatPre(Format1 format1, Format2 format2)
                : base(format1.Name + " / " + format2.Name, format1.Description + "\n/\n" + format2.Description,
                       GenericExtensions.CombineDicts(format1.FormatOptions, format2.FormatOptions),
                       format1.Version + "/" + format2.Version) {
            this.Format1Instance = format1;
            this.Format2Instance = format2;
        }

        public Format1 Format1Instance { get; }
        public Format2 Format2Instance { get; }

        // It has the same argument as the version from ReversedFormat.
        // Can still call the original by casting up to it.
        FormattedResult IOutFormat<FormattedMid, FormattedResult>.Format(FormattedMid model) => this.Format2Instance.Format(model);
        public FormattedMid Parse(FormattedResult formattedModel) => this.Format2Instance.Parse(formattedModel);
    }

    // We have to split ChainFormatPre off because otherwise it will fail to compile since it would
    // technically be possible that the parameters could be specified so that the IFormat interfaces
    // would be the same which is not allowed. Moving them to separate classes makes it work though.
    public sealed class ChainFormat<Format1, T, Mid, FormattedMid, Format2, FormatResult, FormattedResult>
              : ChainFormatPre<Format1, T, Mid, FormattedMid, Format2, FormatResult, FormattedResult>,
                IFormat<T, FormatResult, FormattedResult>
            where FormattedMid : Formatted<Mid>
            where FormattedResult : Formatted<FormatResult>
            where Format1 : IFormat<T, Mid, FormattedMid>
            where Format2 : IFormat<FormattedMid, FormatResult, FormattedResult> {

        public ChainFormat(Format1 format1, Format2 format2) : base(format1, format2) { }

        public override FormattedMid Format(T model) => this.Format1Instance.Format(model);
        public override T Parse(FormattedMid formattedModel) => this.Format1Instance.Parse(formattedModel);
        FormattedResult IOutFormat<T, FormattedResult>.Format(T model) => this.Format2Instance.Format(this.Format1Instance.Format(model));
        T IInFormat<T, FormattedResult>.Parse(FormattedResult formattedModel) => this.Format1Instance.Parse(this.Format2Instance.Parse(formattedModel));

    }
}