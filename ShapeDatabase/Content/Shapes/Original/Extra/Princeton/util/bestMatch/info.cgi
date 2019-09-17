#!/usr/local/bin/perl

use CGI qw/escape unescape/;

require "cgi-lib.pl";

&ReadParse;

$mid = $in{"mid"};
$thn = $in{"thn"};

&write_html_header;

&write_model_info;

print "</body></html>";






sub write_html_header
{
    print "Content-type: text/html\n\n";
    
    print "<html><head><title>Information for [m$mid]</title></head>\n";
    print "<body>\n";
    print "<font face=\"Arial, Helvetica\">\n";

}  # write_html_header



sub write_model_info
{
    print "<table width=\"100%\">\n";
    print "<tr>\n";
    $model_id = $mid;
    $nr_thumbnails = 8;
    $subdir = int($model_id / 100);

    for($i=0; $i < $nr_thumbnails; $i++) {
        
        
        $thumbnail_url[$i] = "http://shape.cs.princeton.edu/benchmark/thumbnails/$subdir/m$model_id/new_small$i.jpg";
        print "<td>\n";
        
        $large_thumbnail = $thumbnail_url[$i];
        $large_thumbnail =~ s/small/large/;
        if (($thn eq "") || ($thn eq "-1")) {
            $info_url = "info.cgi?mid=$mid&thn=$i";
            print "<a href=\"$info_url\"><img src=\"$thumbnail_url[$i]\"></a>\n";
        }
        else {
            if ($i == $thn) {
                $info_url = "javascript:self.back()";
                print "<a href=\"$info_url\"><img src=$large_thumbnail></a>\n";
            }
        }
        print "</td>\n";
        if ($i == 3) {
            print "</tr>\n<tr>";
        }
    }
    print "</tr></table><p>\n";
    
    print "<center><a href=\"javascript:self.close()\"><b>(close)</b></a></center><p>\n";
    
}  # write_model_info





