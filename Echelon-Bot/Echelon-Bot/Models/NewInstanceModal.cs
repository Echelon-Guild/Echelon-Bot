using Discord.Interactions;

namespace EchelonBot
{
    public class NewInstanceModal : IModal
    {
        public string Title => "Add Instance to database";

        [InputLabel("Name")]
        [ModalTextInput("instanceName")]
        public string InstanceName { get; set; }

        [InputLabel("Type")]
        [ModalTextInput("instanceType")]
        public string InstanceType { get; set; }

        [InputLabel("Is this legacy? Say True or False")]
        [ModalTextInput("instanceLegacy")]
        public string InstanceLegacy { get; set; }

        [InputLabel("Image Url for the instance")]
        [ModalTextInput("instanceUrl")]
        public string InstanceUrl { get; set; }
    }

}
