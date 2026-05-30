// Use container fluid
// var containers = $(".container");
// containers.removeClass("container");
// containers.addClass("container-fluid");

WINDOW_CONTENTS = window.location.href.split('/')
SELECTED_LANGUAGE = 'dotnet'
BLOB_URI_PREFIX = 'https://azuresdkdocs.blob.core.windows.net/$web/dotnet/'

ATTR1 = '[<span class="hljs-meta">System.ComponentModel.EditorBrowsable</span>]\n<'

THEME_STORAGE_KEY = 'pnp-docfx-theme'

function readStoredTheme() {
    try {
        var storedTheme = localStorage.getItem(THEME_STORAGE_KEY)
        if (storedTheme === 'dark' || storedTheme === 'light') {
            return storedTheme
        }
    }
    catch (e) {
        // Ignore storage access issues and fall back to system preference.
    }

    return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

function updateThemeToggle(theme) {
    var themeToggle = document.getElementById('theme-toggle')
    if (!themeToggle) {
        return
    }

    var isDarkMode = theme === 'dark'
    var nextMode = isDarkMode ? 'light' : 'dark'

    themeToggle.setAttribute('aria-label', 'Switch to ' + nextMode + ' mode')
    themeToggle.setAttribute('title', 'Switch to ' + nextMode + ' mode')

    var icon = themeToggle.querySelector('.theme-toggle-icon')
    if (icon) {
        icon.className = 'theme-toggle-icon fa ' + (isDarkMode ? 'fa-sun-o' : 'fa-moon-o')
    }
}

function applyTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme)
    updateThemeToggle(theme)
}

function initializeThemeToggle() {
    var initialTheme = readStoredTheme()
    applyTheme(initialTheme)

    var themeToggle = document.getElementById('theme-toggle')
    if (!themeToggle) {
        return
    }

    themeToggle.addEventListener('click', function () {
        var currentTheme = document.documentElement.getAttribute('data-theme') === 'dark' ? 'dark' : 'light'
        var nextTheme = currentTheme === 'dark' ? 'light' : 'dark'

        applyTheme(nextTheme)

        try {
            localStorage.setItem(THEME_STORAGE_KEY, nextTheme)
        }
        catch (e) {
            // Ignore storage access issues and keep runtime-only preference.
        }
    })
}

// Navbar Hamburger
$(function () {
    $(".navbar-toggle").click(function () {
        $(this).toggleClass("change");
    })
})

// Select list to replace affix on small screens
$(function () {
    var navItems = $(".sideaffix .level1 > li");

    if (navItems.length == 0) {
        return;
    }

    var selector = $("<select/>");
    selector.addClass("form-control visible-sm visible-xs");
    var form = $("<form/>");
    form.append(selector);
    form.prependTo("article");

    selector.change(function () {
        window.location = $(this).find("option:selected").val();
    })

    function work(item, level) {
        var link = item.children('a');

        var text = link.text();

        for (var i = 0; i < level; ++i) {
            text = '&nbsp;&nbsp;' + text;
        }

        selector.append($('<option/>', {
            'value': link.attr('href'),
            'html': text
        }));

        var nested = item.children('ul');

        if (nested.length > 0) {
            nested.children('li').each(function () {
                work($(this), level + 1);
            });
        }
    }

    navItems.each(function () {
        work($(this), 0);
    });
})


$(function () {
    // Inject line breaks and spaces into the code sections
    //$(".lang-csharp").each(function () {
    //    var text = $(this).html();
    //    text = text.replace(/, /g, ",</br>&#09;&#09");
    //    text = text.replace(ATTR1, '<');
    //    $(this).html(text);
    //});

    // Add text to empty links
    $("p > a").each(function () {
        var link = $(this).attr('href')
        if ($(this).text() === "") {
            $(this).html(link)
        }
    });
})

$(function () {
    initializeThemeToggle()
})

function httpGetAsync(targetUrl, callback) {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.onreadystatechange = function () {
        if (xmlHttp.readyState == 4 && xmlHttp.status == 200)
            callback(xmlHttp.responseText);
    }
    xmlHttp.open("GET", targetUrl, true); // true for asynchronous 
    xmlHttp.send(null);
}

function populateOptions(selector, packageName) {
    var versionRequestUrl = BLOB_URI_PREFIX + packageName + "/versioning/versions"

    httpGetAsync(versionRequestUrl, function (responseText) {
        var versionselector = document.createElement("select")
        var cv = WINDOW_CONTENTS[6]

        versionselector.className = 'navbar-version-select'
        if (responseText) {
            options = responseText.match(/[^\r\n]+/g)
            for (var i in options) {
                $(versionselector).append('<option value="' + options[i] + '">' + options[i] + '</option>')
            }
        }

        if(cv === 'latest')
        {
            $(versionselector).selectedIndex = 0
        }
        else {
            $(versionselector).val(cv);
        }
        
        $(selector).append(versionselector)

        $(versionselector).change(function () {
            targetVersion = $(this).val()
            url = WINDOW_CONTENTS.slice()
            url[6] = targetVersion
            window.location.href = url.join('/')
        });

    })
}


function populateIndexList(selector, packageName) {
    url = BLOB_URI_PREFIX + packageName + "/versioning/versions"

    httpGetAsync(url, function (responseText) {

        var publishedversions = document.createElement("ul")
        if (responseText) {
            options = responseText.match(/[^\r\n]+/g)

            for (var i in options) {
                $(publishedversions).append('<li><a href="' + getPackageUrl(SELECTED_LANGUAGE, packageName, options[i]) + '" target="_blank">' + options[i] + '</a></li>')
            }
        }
        else {
            $(publishedversions).append('<li>No discovered versions present in blob storage.</li>')
        }
        $(selector).after(publishedversions)
    })
}

function getPackageUrl(language, package, version) {
    return "https://azuresdkdocs.blob.core.windows.net/$web/" + language + "/" + package + "/" + version + "/api/index.html"
}

// Populate Versions
$(function () {
    if (WINDOW_CONTENTS.length < 7 && WINDOW_CONTENTS[WINDOW_CONTENTS.length - 1] != 'index.html') {
        console.log("Run PopulateList")

        $('h4').each(function () {
            var pkgName = $(this).text()
            populateIndexList($(this), pkgName)
        })
    }

    if (WINDOW_CONTENTS.length > 7) {
        var pkgName = WINDOW_CONTENTS[5]
        populateOptions($('#navbar'), pkgName)
    }
})

// For the demos section that is generated at runtime, 
// fix for the pencil referencing section that does not yet exist
$(function (){
    $("a.improve-doc-lg").each(function () {
        var link = $(this).attr('href');
        if(link.indexOf("/dev/docs/demos/") > -1){
            link = link.replace("/dev/docs/demos/","/dev/samples/");
            $(this).attr('href', link);
        }
    });
});

$(function () {
    var copyToClipboard = function (text) {
        // Create a textblock and assign the text and add to document
        var el = document.createElement('textarea');
        el.value = text;
        document.body.appendChild(el);
        el.style.display = "block";

        // select the entire textblock
        el.select();

        // copy to clipboard
        document.execCommand('copy');

        // clean up element
        document.body.removeChild(el);
    }

    $("code.hljs").each(function () {
        var $this = $(this);
        var match = /lang-(.+?)(\s|$)/.exec($this.attr("class"));
        if (!match) {
            return;
        }
        var language = match[1].toUpperCase();
        if (language === 'CS') {
            language = "C#";
        }
        if (language === 'JS') {
            language = "JavaScript";
        }
        if (language === 'POWERSHELL') {
            language = "PowerShell"
        }
        if (language === 'BAT') {
            language = "Bat"
        }
        if (language === 'BASH') {
            language = "Bash"
        }
        if (language !== 'YAML') {
            var $codeHeader = $(
                '<div class="code-header">' +
                '    <span class="language">' + language + '</span>' +
                '    <button type="button" class="action" aria-label="Copy code">' +
                '		<span class="icon"><span class="glyphicon glyphicon-duplicate" role="presentation"></span></span>' +
                '		<span>Copy</span>' +
                '		<div class="successful-copy-alert is-transparent" aria-hidden="true">' +
                '			<span class="icon is-size-large">' +
                '				<span class="glyphicon glyphicon-ok" role="presentation"></span>' +
                '			</span>' +
                '		</div>' +
                '	</button>' +
                '</div>'
            );
            $this.closest("pre").before($codeHeader);
            $codeHeader.find("button").click(function () {
                copyToClipboard($this.closest("pre").text());
                var successAlert = $(this).find(".successful-copy-alert");
                successAlert.removeClass("is-transparent");
                setTimeout(function () { successAlert.addClass("is-transparent"); }, 2000);
            });
        }
    });
});
