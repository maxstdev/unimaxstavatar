
#import <Foundation/Foundation.h>

extern "C" {
    bool IsAppInstalled(const char* urlScheme) {
        NSString *scheme = [NSString stringWithUTF8String:urlScheme];
        NSURL *url = [NSURL URLWithString:[NSString stringWithFormat:@"%@://", scheme]];
        return [[UIApplication sharedApplication] canOpenURL:url];
    }
}
