using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using K2Calendar.Models;
using System.Web.Mvc;
using System.Security.Policy;

namespace K2Calendar.Views.Shared.Helpers
{
    public class CustomHtmlHelpers
    {
        public static MvcHtmlString TagsListFor(PostModel postModel, bool isDisabled, string theme)
        {
            string existingTags = FormatExistingTags(postModel);
            return new MvcHtmlString(string.Format(@"<strong><small>Tags:</small></strong>
                            <input type='text' id='TagsInput{0}' name='TagsInput' />
                            <script type='text/javascript'>
                    $(document).ready(function () {{
                        $('#TagsInput{0}').tokenInput('{1}', {{
                            theme: '{2}',
                            allowFreeTagging: true,
                            preventDuplicates: true,
                            hintText: ""e.g. 'armbar' 'back mount' 'Barrhaven' ..."",
                            noResultsText: 'Nothing found. Press <Enter> to add this new tag.',
                            searchingText: 'Searching...',
                            disabled: {3},   
                            {4}     
                        }});
                    }});</script>", postModel.Id, "/Post/SearchTags", theme, isDisabled.ToString().ToLower(), existingTags));
        }

        /// <summary>
        /// Creates the pre-populated tags property for the JQuery TokenInput control used to render Tags for a Post
        /// </summary>
        private static string FormatExistingTags(PostModel model)
        {
            string existingTags = "";
            if (model.Tags != null || model.Tags.Count > 0)
            {
                System.Text.StringBuilder prePopulate = new System.Text.StringBuilder("prePopulate: [ \n");
                foreach (var tag in model.Tags)
                {
                    string tagString = string.Format(@"{{id:{0}, name:""{1}""}},", tag.Id, tag.Name);
                    prePopulate.Append(tagString);
                }
                prePopulate.Remove(prePopulate.Length - 1, 1); //remove trailing comma
                prePopulate.Append("]");
                existingTags = prePopulate.ToString();
            }
            return existingTags;
        }

    }
}