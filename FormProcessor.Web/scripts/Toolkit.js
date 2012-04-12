/*
Copied from https://code.google.com/p/fbug/source/browse/branches/firebug1.5/lite/firebugx.js
to handle scenarios where the Firebug Console is not present or active/enabled.
*/
if (!window.console || !console.firebug) {
	var names = ["log", "debug", "info", "warn", "error", "assert", "dir", "dirxml",
	"group", "groupEnd", "time", "timeEnd", "count", "trace", "profile", "profileEnd"];

	window.console = {};
	for (var i = 0; i < names.length; ++i)
	window.console[names[i]] = function () { }
}
