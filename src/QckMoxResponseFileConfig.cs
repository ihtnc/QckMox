namespace QckMox
{
    public class QckMoxResponseFileConfig : QckMoxResponseConfig
    {
        public bool? Base64Content { get; set; }

        internal static QckMoxResponseFileConfig Copy(QckMoxResponseConfig config) => new QckMoxResponseFileConfig
        {
            ContentInProp = config?.ContentInProp,
            ContentType = config?.ContentType,
            FileContentProp = config?.FileContentProp,
            Headers = config?.Headers
        };
    }
}