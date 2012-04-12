<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:ms="urn:schemas-microsoft-com:xslt"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								exclude-result-prefixes="msxsl">
	<xsl:output method="html" indent="yes"/>

	<xsl:template match="/formData">
		<h3>
			Online form submitted on
			<xsl:value-of select="ms:format-date(meta/@datetime, 'MMM dd, yyyy')"/> -
			<xsl:value-of select="ms:format-time(meta/@datetime, 'hh:mm tt')"/>
		</h3>
		<xsl:apply-templates select="data/fields/field"/>
	</xsl:template>
	
	<xsl:template match="data/fields/field">
		<h4>
			<xsl:value-of select="@name"/>:
		</h4>
		<p>
			<xsl:value-of select="text()"/>
		</p>
	</xsl:template>
</xsl:stylesheet>
