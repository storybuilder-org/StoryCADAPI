// Turns each <div class="code-tabs"> containing two or more fenced code blocks
// into a tabbed switcher. Tab labels are derived from each block's language
// class (language-csharp -> "C#", language-python -> "Python", ...), so the
// Markdown only needs to wrap the fenced blocks in the container.
(function () {
  "use strict";

  var LABELS = {
    csharp: "C#", cs: "C#",
    python: "Python", py: "Python",
    bash: "Shell", sh: "Shell", shell: "Shell", console: "Console",
    powershell: "PowerShell", ps: "PowerShell",
    json: "JSON", xml: "XML", yaml: "YAML", yml: "YAML",
    text: "Text", plaintext: "Text"
  };

  function labelFor(lang) {
    if (!lang) return "Code";
    if (LABELS[lang]) return LABELS[lang];
    return lang.charAt(0).toUpperCase() + lang.slice(1);
  }

  function langOf(block) {
    var m = (block.className || "").match(/language-([\w-]+)/);
    return m ? m[1] : "";
  }

  function init() {
    var groups = document.querySelectorAll(".code-tabs");
    Array.prototype.forEach.call(groups, function (group) {
      if (group.getAttribute("data-tabbed")) return;

      var blocks = Array.prototype.filter.call(group.children, function (el) {
        return el.classList && el.classList.contains("highlighter-rouge");
      });
      if (blocks.length < 2) return;

      group.setAttribute("data-tabbed", "1");

      // Optional explicit labels: data-tabs="C#|Python" (used when both tabs
      // share a language, e.g. shell commands). Otherwise derive from language.
      var override = group.getAttribute("data-tabs");
      var labels = override ? override.split("|") : null;

      var bar = document.createElement("div");
      bar.className = "code-tab-bar";

      blocks.forEach(function (block, i) {
        var btn = document.createElement("button");
        btn.type = "button";
        btn.className = "code-tab-btn" + (i === 0 ? " active" : "");
        btn.textContent = (labels && labels[i] != null)
          ? labels[i].trim()
          : labelFor(langOf(block));
        btn.addEventListener("click", function () {
          blocks.forEach(function (b, j) { b.hidden = j !== i; });
          var btns = bar.querySelectorAll(".code-tab-btn");
          Array.prototype.forEach.call(btns, function (x, j) {
            x.classList.toggle("active", j === i);
          });
        });
        bar.appendChild(btn);
        block.hidden = i !== 0;
      });

      group.insertBefore(bar, group.firstChild);
    });
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }
})();
