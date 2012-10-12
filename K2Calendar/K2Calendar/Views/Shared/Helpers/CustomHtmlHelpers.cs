using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using K2Calendar.Models;
using System.Web.Mvc;
using System.Security.Policy;
using System.Text;

namespace K2Calendar.Views.Shared.Helpers
{
    public class CustomHtmlHelpers
    {
        public static MvcHtmlString TagsListFor(PostModel postModel, bool isDisabled, string theme)
        {
            string existingTags = FormatExistingTags(postModel);
            int inputId = (postModel != null) ? postModel.Id : 1;
            string closeChar = (isDisabled) ? "" : "x";
            return new MvcHtmlString(string.Format(@"
                            <input type='text' id='TagsInput{0}' name='TagsInput' />
                            <script type='text/javascript'>
                    $(document).ready(function () {{
                        $('#TagsInput{0}').tokenInput('{1}', {{
                            theme: '{2}',
                            deleteText: '{3}',
                            allowFreeTagging: true,
                            preventDuplicates: true,
                            hintText: ""e.g. 'Armbar' 'BackMount' 'Barrhaven' ..."",
                            noResultsText: 'Nothing found. Press <Enter> to add this new tag.',
                            searchingText: 'Searching...',
                            disabled: {4},   
                            {5}    
                        }});
                    }});</script>", inputId, "/Post/SearchTags", theme, closeChar, isDisabled.ToString().ToLower(), existingTags));
        }

        public static MvcHtmlString SubmitButton(string label)
        {
            return new MvcHtmlString(string.Format(@"<button type='submit' class='btn btn-primary btn-large'>{0} &raquo;</button>", label));
        }

        public static HtmlString HtmlEnabledTextAreaFor(string id, string text)
        {
            return new HtmlString(string.Format(@"<textarea cols='20' id='{0}' name='{0}' rows='10' style='width:80%'>{1}</textarea>", id, HttpUtility.HtmlDecode(text)));
        }

        /// <summary>
        /// Creates the pre-populated tags property for the JQuery TokenInput control used to render Tags for a Post
        /// </summary>
        private static string FormatExistingTags(PostModel model)
        {
            StringBuilder prePopulate = new StringBuilder("prePopulate: [ \n");
            if (model != null && model.Tags != null && model.Tags.Count > 0)
            {
                foreach (var tag in model.Tags)
                {
                    string tagString = string.Format(@"{{id:{0}, name:""{1}""}},", tag.Id, tag.Name);
                    prePopulate.Append(tagString);
                }
                prePopulate.Remove(prePopulate.Length - 1, 1); //remove trailing comma                
            }
            prePopulate.Append("]");
            return prePopulate.ToString();
        }

    }
}